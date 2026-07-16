using DotCruz.Notifications.Domain.Entities.Notifications;
using DotCruz.Notifications.Domain.Enums.Notifications;
using DotCruz.Notifications.Domain.Interfaces;

namespace DotCruz.Notifications.Application.Factories.Notifications;

public class PushFactoryStrategy : INotificationFactoryStrategy
{
    public NotificationType Type => NotificationType.Push;

    public Notification Create(
        string serviceName,
        string recipient,
        string? culture,
        string? body,
        string? title,
        Guid? templateId,
        Dictionary<string, object>? templateData,
        DateTimeOffset? scheduledFor,
        Guid tenantId)
    {
        return new PushNotification(
            serviceName,
            recipient,
            culture,
            title ?? string.Empty,
            body,
            templateId,
            templateData,
            scheduledFor,
            tenantId);
    }
}
