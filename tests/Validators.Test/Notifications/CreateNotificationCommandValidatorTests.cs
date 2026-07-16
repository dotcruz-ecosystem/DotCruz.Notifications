using CommonTestUtilities.Commands.Notifications;
using DotCruz.Notifications.Application.UseCases.Notifications.CreateNotification;
using DotCruz.Notifications.Contracts.Enums.Notifications;
using DotCruz.Notifications.Domain.Exceptions.Resources;

namespace Validators.Test.Notifications;

public class CreateNotificationCommandValidatorTests
{
    [Fact]
    public async Task Success()
    {
        var validator = new CreateNotificationCommandValidator();
        var request = CreateNotificationCommandBuilder.Build();

        var result = await validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Success_With_TemplateCode()
    {
        var validator = new CreateNotificationCommandValidator();
        var request = CreateNotificationCommandBuilder.Build(body: string.Empty, templateCode: "WELCOME");

        var result = await validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        Assert.True(result.IsValid);
    }



    [Fact]
    public async Task Error_Recipient_Empty()
    {
        var validator = new CreateNotificationCommandValidator();
        var request = CreateNotificationCommandBuilder.Build(recipient: string.Empty);

        var result = await validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Equals(ResourceMessagesException.RECIPIENT_EMPTY));
    }

    [Fact]
    public async Task Error_Type_Invalid()
    {
        var validator = new CreateNotificationCommandValidator();
        var request = CreateNotificationCommandBuilder.Build(type: (IntegrationNotificationType)999);

        var result = await validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Equals(ResourceMessagesException.NOTIFICATION_TYPE_INVALID));
    }

    [Fact]
    public async Task Error_Body_And_Template_Empty()
    {
        var validator = new CreateNotificationCommandValidator();
        var request = CreateNotificationCommandBuilder.Build(body: string.Empty, templateCode: null);

        var result = await validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Equals(ResourceMessagesException.BODY_OR_TEMPLATE_REQUIRED));
    }
}
