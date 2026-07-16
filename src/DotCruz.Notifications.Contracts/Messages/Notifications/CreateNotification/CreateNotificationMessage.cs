using DotCruz.Notifications.Contracts.Enums.Notifications;

namespace DotCruz.Notifications.Contracts.Messages.Notifications.CreateNotification;

public record CreateNotificationMessage(
    IntegrationNotificationType Type = default,
    string Recipient = "",
    string? Culture = null,
    string? Body = null,
    string? Title = null,
    string? TemplateCode = null,
    Dictionary<string, object>? TemplateData = null,
    DateTimeOffset? ScheduledFor = null
);
