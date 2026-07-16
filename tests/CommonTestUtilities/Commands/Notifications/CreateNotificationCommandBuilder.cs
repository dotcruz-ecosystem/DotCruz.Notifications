using Bogus;
using DotCruz.Notifications.Application.UseCases.Notifications.CreateNotification;
using DotCruz.Notifications.Contracts.Enums.Notifications;
using DotCruz.Notifications.Contracts.Messages.Notifications.CreateNotification;

namespace CommonTestUtilities.Commands.Notifications;

public class CreateNotificationCommandBuilder
{
    public static CreateNotificationCommand Build(
        Guid? serviceId = null,
        IntegrationNotificationType? type = null,
        string? recipient = null,
        string? culture = null,
        string? body = null,
        string? title = null,
        string? templateCode = null,
        Dictionary<string, object>? templateData = null,
        DateTimeOffset? scheduledFor = null)
    {
        var f = new Faker();

        var message = new CreateNotificationMessage(
            Type: type ?? f.PickRandom<IntegrationNotificationType>(),
            Recipient: recipient ?? f.Internet.Email(),
            Culture: culture,
            Body: body ?? (templateCode != null ? null : f.Lorem.Paragraph()),
            Title: title ?? f.Lorem.Sentence(),
            TemplateCode: templateCode,
            TemplateData: templateData,
            ScheduledFor: scheduledFor
        );

        return new CreateNotificationCommand(message);
    }
}
