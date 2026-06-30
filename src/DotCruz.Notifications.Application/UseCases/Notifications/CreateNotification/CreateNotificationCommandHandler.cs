using DotCruz.Notifications.Application.Common.Interfaces;
using DotCruz.Notifications.Application.Common.Utils;
using DotCruz.Notifications.Contracts.Enums.Notifications;
using DotCruz.Notifications.Contracts.Messages.Notifications.SendNotification;
using DotCruz.Notifications.Domain.Entities.Notifications;
using DotCruz.Notifications.Domain.Enums.Notifications;
using DotCruz.Notifications.Domain.Exceptions.BaseExceptions;
using DotCruz.Notifications.Domain.Exceptions.Resources;
using DotCruz.Notifications.Domain.Interfaces;
using DotCruz.Notifications.Domain.Interfaces.Repositories;
using MediatR;

namespace DotCruz.Notifications.Application.UseCases.Notifications.CreateNotification;

public class CreateNotificationCommandHandler : IRequestHandler<CreateNotificationCommand, Guid>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly ITemplateRepository _templateRepository;
    private readonly ITenantSettingsRepository _tenantSettingsRepository;
    private readonly IEnumerable<INotificationFactoryStrategy> _factories;
    private readonly IPublishNotificationService _publishService;
    private readonly ITemplateEngine _templateEngine;
    private readonly INotificationScheduler _notificationScheduler;
    private readonly ITenantProvider _tenantProvider;

    public CreateNotificationCommandHandler(
        INotificationRepository notificationRepository,
        ITemplateRepository templateRepository,
        ITenantSettingsRepository tenantSettingsRepository,
        IEnumerable<INotificationFactoryStrategy> factories,
        IPublishNotificationService publishService,
        ITemplateEngine templateEngine,
        INotificationScheduler notificationScheduler,
        ITenantProvider tenantProvider)
    {
        _notificationRepository = notificationRepository;
        _templateRepository = templateRepository;
        _tenantSettingsRepository = tenantSettingsRepository;
        _factories = factories;
        _publishService = publishService;
        _templateEngine = templateEngine;
        _notificationScheduler = notificationScheduler;
        _tenantProvider = tenantProvider;
    }

    public async Task<Guid> Handle(CreateNotificationCommand request, CancellationToken cancellationToken)
    {
        var message = request.Message;

        var resolvedTemplateId = await ResolveTemplateAsync(message.TemplateCode, message.Culture, cancellationToken);

        var domainType = MapToDomainType(message.Type);

        var factory = _factories.FirstOrDefault(f => f.Type == domainType)
            ?? throw new NotificationTypeNotSupportedException();

        var tenantId = _tenantProvider.TenantId;
        if (!tenantId.HasValue)
            throw new UnauthorizedException(ResourceMessagesException.TENANT_ID_REQUIRED);

        var notification = factory.Create(
            message.ServiceId,
            message.Recipient,
            message.Culture,
            message.Body,
            message.Title,
            resolvedTemplateId,
            message.TemplateData,
            message.ScheduledFor,
            tenantId.Value);

        await ProcessTemplateAsync(notification, cancellationToken);

        await _notificationRepository.AddAsync(notification, cancellationToken);

        await SendNotification(notification, message.Type, cancellationToken);

        return notification.Id;
    }

    private async Task ProcessTemplateAsync(Notification notification, CancellationToken cancellationToken)
    {
        var (rawTitle, rawBody) = await GetRawContent(notification, cancellationToken);

        if (!string.IsNullOrWhiteSpace(rawTitle))
        {
            var renderedTitle = _templateEngine.Render(rawTitle, notification.TemplateData);
            notification.SetRenderedTitle(renderedTitle);
        }

        if (!string.IsNullOrWhiteSpace(rawBody))
        {
            var renderedBody = _templateEngine.Render(rawBody, notification.TemplateData);

            if (notification.Type == NotificationType.Email)
            {
                var wrapped = false;
                if (notification.TenantId != Guid.Empty)
                {
                    var tenantSettings = await _tenantSettingsRepository.GetByTenantIdAsync(notification.TenantId, cancellationToken);
                    if (tenantSettings != null)
                    {
                        var (header, footer) = EmailBrandingBuilder.GenerateBranding(tenantSettings, notification.Culture);
                        renderedBody = $"{header}{renderedBody}{footer}";
                        wrapped = true;
                    }
                }

                if (!wrapped)
                    renderedBody = EmailTemplateWrapper.Wrap(renderedBody);
            }

            notification.SetRenderedBody(renderedBody);
        }
    }

    private async Task<(string Title, string Body)> GetRawContent(Notification notification, CancellationToken cancellationToken)
    {
        if (notification.TemplateId.HasValue)
        {
            var template = await _templateRepository.GetByIdAsync(notification.TemplateId.Value, cancellationToken);
            if (template != null)
                return (template.DefaultTitle, template.Body);
        }

        var title = notification switch
        {
            EmailNotification e => e.Title,
            PushNotification p => p.Title,
            _ => string.Empty
        };

        return (title!, notification.Body!);
    }

    private async Task<Guid?> ResolveTemplateAsync(string? code, string? culture, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(code))
            return null;

        var template = await _templateRepository.GetByCodeAsync(code, culture ?? "pt-BR", cancellationToken);
        if (template == null && _tenantProvider.TenantId.HasValue)
            template = await _templateRepository.GetGlobalByCodeAsync(code, culture ?? "pt-BR", cancellationToken);
        
        if (template == null && culture != "pt-BR")
        {
            template = await _templateRepository.GetByCodeAsync(code, "pt-BR", cancellationToken);
            if (template == null && _tenantProvider.TenantId.HasValue)
                template = await _templateRepository.GetGlobalByCodeAsync(code, "pt-BR", cancellationToken);
        }

        if (template == null && culture != "en" && culture != "pt-BR")
        {
            template = await _templateRepository.GetByCodeAsync(code, "en", cancellationToken);
            if (template == null && _tenantProvider.TenantId.HasValue)
                template = await _templateRepository.GetGlobalByCodeAsync(code, "en", cancellationToken);
        }

        if (template == null)
            throw new NotFoundException(ResourceMessagesException.TEMPLATE_NOT_FOUND);

        return template.Id;
    }

    private static NotificationType MapToDomainType(IntegrationNotificationType type)
    {
        return type switch
        {
            IntegrationNotificationType.Email => NotificationType.Email,
            IntegrationNotificationType.Sms => NotificationType.Sms,
            IntegrationNotificationType.Push => NotificationType.Push,
            _ => throw new NotificationTypeNotSupportedException()
        };
    }

    private async Task SendNotification(Notification notification, IntegrationNotificationType type, CancellationToken cancellationToken)
    {
        var messagePayload = BuildNotificationMessage(notification, type);

        if (notification.ScheduledFor.HasValue && notification.ScheduledFor.Value > DateTimeOffset.UtcNow)
        {
            await _notificationScheduler.ScheduleAsync(messagePayload, notification.ScheduledFor.Value, cancellationToken);
        }
        else
        {
            await _publishService.PublishNotificationCreatedEvent(messagePayload, cancellationToken);
        }
    }

    private static SendNotificationMessage BuildNotificationMessage(Notification notification, IntegrationNotificationType type)
    {
        var title = notification switch
        {
            EmailNotification e => e.Title,
            PushNotification p => p.Title,
            _ => null
        };

        return new SendNotificationMessage(notification.Id, type, notification.Recipient, notification.Body!, title, notification.TenantId);
    }
}
