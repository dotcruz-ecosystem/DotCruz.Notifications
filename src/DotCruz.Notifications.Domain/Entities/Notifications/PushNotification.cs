using DotCruz.Notifications.Domain.Enums.Notifications;
using DotCruz.Notifications.Domain.Exceptions.Resources;

namespace DotCruz.Notifications.Domain.Entities.Notifications;

public class PushNotification : Notification
{
    public string Title { get; private set; } = string.Empty;

    private PushNotification() { }

    public PushNotification(
        string serviceName,
        string deviceToken,
        string? culture,
        string title,
        string? body,
        Guid? templateId,
        Dictionary<string, object>? templateData,
        DateTimeOffset? scheduledFor,
        Guid tenantId)
        : base(serviceName, NotificationType.Push, deviceToken, culture, body, templateId, templateData, scheduledFor, tenantId)
    {
        Title = title;
        Validate();
    }

    public override void SetRenderedTitle(string title)
    {
        Title = title;
        Validate();
    }

    protected override void ValidateSpecificRules(List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(Title))
            errors.Add(ResourceMessagesException.TITLE_EMPTY);
    }
}
