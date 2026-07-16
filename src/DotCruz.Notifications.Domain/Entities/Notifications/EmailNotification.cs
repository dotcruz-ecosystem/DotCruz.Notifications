using DotCruz.Notifications.Domain.Enums.Notifications;
using DotCruz.Notifications.Domain.Exceptions.Resources;

namespace DotCruz.Notifications.Domain.Entities.Notifications;

public class EmailNotification : Notification
{
    public string Title { get; private set; } = string.Empty;

    private EmailNotification() { }

    public EmailNotification(
        string serviceName,
        string recipient,
        string? culture,
        string title,
        string? body,
        Guid? templateId,
        Dictionary<string, object>? templateData,
        DateTimeOffset? scheduledFor,
        Guid tenantId)
        : base(serviceName, NotificationType.Email, recipient, culture, body, templateId, templateData, scheduledFor, tenantId)
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
        if (string.IsNullOrWhiteSpace(Title) && TemplateId == null)
            errors.Add(ResourceMessagesException.TITLE_EMPTY);
    }
}
