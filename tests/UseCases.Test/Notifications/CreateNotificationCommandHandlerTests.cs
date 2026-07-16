using CommonTestUtilities.Commands.Notifications;
using CommonTestUtilities.Entities;
using CommonTestUtilities.Entities.Notifications;
using CommonTestUtilities.Entities.Templates;
using DotCruz.Notifications.Application.Common.Utils;
using CommonTestUtilities.Factories;
using CommonTestUtilities.InlineData;
using CommonTestUtilities.Repositories;
using CommonTestUtilities.Services;
using DotCruz.Notifications.Application.UseCases.Notifications.CreateNotification;
using DotCruz.Notifications.Contracts.Enums.Notifications;
using DotCruz.Notifications.Domain.Entities.Templates;
using DotCruz.Notifications.Domain.Enums.Notifications;
using DotCruz.Notifications.Domain.Exceptions.BaseExceptions;
using DotCruz.Notifications.Domain.Exceptions.Resources;
using DotCruz.Notifications.Domain.Interfaces;
using DotCruz.Notifications.Domain.Interfaces.Repositories;
using DotCruz.Shared.Security.Context;
using Moq;
using DotCruz.Notifications.Application.Common.Interfaces.Messaging;
using DotCruz.Notifications.Application.Common.Interfaces.Tenants;

namespace UseCases.Test.Notifications;

public class CreateNotificationCommandHandlerTests
{
    [Theory]
    [ClassData(typeof(IntegrationNotificationTypeInlineDataTest))]
    public async Task Success(IntegrationNotificationType type)
    {
        var command = CreateNotificationCommandBuilder.Build(type: type);
        
        var domainType = type switch
        {
            IntegrationNotificationType.Email => NotificationType.Email,
            IntegrationNotificationType.Sms => NotificationType.Sms,
            IntegrationNotificationType.Push => NotificationType.Push,
            _ => throw new ArgumentOutOfRangeException()
        };

        var notification = NotificationBuilder.Build(domainType);
        
        var strategy = new NotificationFactoryStrategyBuilder(domainType)
            .Create(notification)
            .Build();
            
        var strategies = new NotificationFactoryStrategyListBuilder()
            .Add(strategy)
            .Build();

        var handler = CreateHandler(strategies);

        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        Assert.Equal(notification.Id, result);
    }

    [Fact]
    public async Task Success_With_Template()
    {
        var templateCode = "Welcome";
        var culture = "pt-BR";
        var command = CreateNotificationCommandBuilder.Build(type: IntegrationNotificationType.Email, templateCode: templateCode, culture: culture);
        var template = TemplateBuilder.Build(code: templateCode, culture: culture);
        
        var notification = NotificationBuilder.Build(NotificationType.Email);
        var strategy = new NotificationFactoryStrategyBuilder(NotificationType.Email)
            .Create(notification)
            .Build();
        var strategies = new NotificationFactoryStrategyListBuilder()
            .Add(strategy)
            .Build();

        var handler = CreateHandler(strategies, template);

        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        Assert.Equal(notification.Id, result);
    }

    [Fact]
    public async Task Error_NotificationTypeNotSupported()
    {
        var command = CreateNotificationCommandBuilder.Build();
        
        var handler = CreateHandler();

        Task act() => handler.Handle(command, TestContext.Current.CancellationToken);

        var exception = await Assert.ThrowsAsync<NotificationTypeNotSupportedException>(act);

        Assert.Contains(ResourceMessagesException.NOTIFICATION_TYPE_NOT_SUPPORTED, exception.GetErrorsMessages());
    }

    [Theory]
    [InlineData("pt-BR", "Você está recebendo este e-mail porque possui um cadastro em", "Descadastrar")]
    [InlineData("en", "You are receiving this email because you are registered at", "Unsubscribe")]
    [InlineData("es", "You are receiving this email because you are registered at", "Unsubscribe")] // Fallback to en
    public async Task Success_With_TenantBranding_Internationalized(string culture, string expectedReceivingText, string expectedUnsubscribeText)
    {
        var tenantId = Guid.NewGuid();
        var command = CreateNotificationCommandBuilder.Build(
            type: IntegrationNotificationType.Email,
            culture: culture);

        var notification = EmailNotificationBuilder.Build(
            culture: culture,
            tenantId: tenantId
        );

        var tenantAddress = new TenantAddressDto("Street 1", "123", null, "Neighborhood", "City", "SP", "12345678");
        var tenantBranding = new TenantBrandingDto("https://logo.png", "#ffffff", "https://shop.com", "https://shop.com/unsub");
        var tenantDto = new TenantDto(
            tenantId,
            Name: "Shop Test",
            Slug: "shop-test",
            Status: "Active",
            TenantAddress: tenantAddress,
            TenantBranding: tenantBranding
        );

        var strategy = new NotificationFactoryStrategyBuilder(NotificationType.Email)
            .Create(notification)
            .Build();
        var strategies = new NotificationFactoryStrategyListBuilder()
            .Add(strategy)
            .Build();

        var repository = new NotificationRepositoryBuilder().Build();
        var templateRepository = new TemplateRepositoryBuilder().Build();
        var publishService = new PublishNotificationServiceBuilder().Build();
        
        var templateEngine = new Mock<ITemplateEngine>();
        templateEngine.Setup(x => x.Render(It.IsAny<string>(), It.IsAny<Dictionary<string, object>?>()))
            .Returns<string, Dictionary<string, object>?>((raw, data) => raw);

        var scheduler = new Mock<INotificationScheduler>();
        
        var tenantClient = new Mock<ITenantClient>();
        tenantClient.Setup(x => x.GetTenantByIdAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenantDto);

        var securityContext = new Mock<ISecurityContext>();
        securityContext.Setup(t => t.TenantId).Returns(tenantId);
        securityContext.Setup(t => t.ServiceName).Returns("test-service");

        var handler = new CreateNotificationCommandHandler(
            repository,
            templateRepository,
            tenantClient.Object,
            strategies,
            publishService,
            templateEngine.Object,
            scheduler.Object,
            securityContext.Object);

        var result = await handler.Handle(command, TestContext.Current.CancellationToken);

        Assert.Equal(notification.Id, result);
        Assert.NotNull(notification.Body);
        Assert.Contains(expectedReceivingText, notification.Body);
        Assert.Contains(expectedUnsubscribeText, notification.Body);
        Assert.Contains("Shop Test", notification.Body);
    }

    private static CreateNotificationCommandHandler CreateHandler(IEnumerable<INotificationFactoryStrategy>? strategies = null, Template? template = null)
    {
        strategies ??= new NotificationFactoryStrategyListBuilder().Build();

        var repository = new NotificationRepositoryBuilder().Build();
        var templateRepository = new TemplateRepositoryBuilder();
        var publishService = new PublishNotificationServiceBuilder().Build();
        var templateEngine = new Mock<ITemplateEngine>();
        
        templateEngine.Setup(x => x.Render(It.IsAny<string>(), It.IsAny<Dictionary<string, object>?>()))
            .Returns<string, Dictionary<string, object>?>((raw, data) => raw);

        var scheduler = new Mock<INotificationScheduler>();
        var tenantClient = new Mock<ITenantClient>();
        var securityContext = new Mock<ISecurityContext>();
        securityContext.Setup(t => t.TenantId).Returns(Guid.NewGuid());
        securityContext.Setup(t => t.ServiceName).Returns("test-service");

        if (template != null)
        {
            templateRepository.GetById(template);
            templateRepository.GetByCode(template);
        }

        return new CreateNotificationCommandHandler(
            repository,
            templateRepository.Build(),
            tenantClient.Object,
            strategies,
            publishService,
            templateEngine.Object,
            scheduler.Object,
            securityContext.Object);
    }
}
