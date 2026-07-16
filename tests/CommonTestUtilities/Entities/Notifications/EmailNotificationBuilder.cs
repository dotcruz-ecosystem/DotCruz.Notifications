using System;
using Bogus;
using DotCruz.Notifications.Domain.Entities.Notifications;

namespace CommonTestUtilities.Entities.Notifications;

public class EmailNotificationBuilder
{
    public static EmailNotification Build(
        string? serviceName = null,
        string? recipient = null,
        string? culture = null,
        string? title = null,
        string? body = null,
        Guid? templateId = null,
        Dictionary<string, object>? templateData = null,
        DateTimeOffset? scheduledFor = null,
        Guid? tenantId = null)
    {
        var f = new Faker();

        if (body is null && templateId is null)
            body = f.Lorem.Paragraph();

        return new EmailNotification(
            serviceName: serviceName ?? f.Random.Word(),
            recipient: recipient ?? f.Internet.Email(),
            culture: culture ?? "pt-BR",
            title: title ?? f.Lorem.Sentence(),
            body: body,
            templateId: templateId,
            templateData: templateData,
            scheduledFor: scheduledFor ?? f.Date.FutureOffset(),
            tenantId: tenantId ?? f.Random.Guid()
        );
    }
}
