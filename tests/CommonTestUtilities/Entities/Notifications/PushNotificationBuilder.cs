using System;
using Bogus;
using DotCruz.Notifications.Domain.Entities.Notifications;

namespace CommonTestUtilities.Entities.Notifications;

public class PushNotificationBuilder
{
    public static PushNotification Build(
        string? serviceName = null,
        string? deviceToken = null,
        string? culture = null,
        string? title = null,
        string? body = null,
        Guid? templateId = null,
        Dictionary<string, object>? templateData = null,
        DateTimeOffset? scheduledFor = null,
        Guid? tenantId = null)
    {
        var faker = new Faker<PushNotification>()
            .CustomInstantiator(f => new PushNotification(
                    serviceName: serviceName ?? f.Random.Word(),
                    deviceToken: deviceToken ?? f.Random.Guid().ToString(),
                    culture: culture ?? f.PickRandom("pt-BR", "en-US", "es-ES"),
                    title: title ?? f.Lorem.Sentence(),
                    body: body ?? f.Lorem.Paragraph(),
                    templateId: templateId,
                    templateData: templateData,
                    scheduledFor: scheduledFor ?? f.Date.FutureOffset(),
                    tenantId: tenantId ?? f.Random.Guid()
                )
            );

        return faker.Generate();
    }
}
