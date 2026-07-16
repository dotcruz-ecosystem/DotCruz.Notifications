using System;
using Bogus;
using DotCruz.Notifications.Domain.Entities.Notifications;

namespace CommonTestUtilities.Entities.Notifications;

public class SmsNotificationBuilder
{
    public static SmsNotification Build(
        string? serviceName = null,
        string? phoneNumber = null,
        string? culture = null,
        string? body = null,
        Guid? templateId = null,
        Dictionary<string, object>? templateData = null,
        DateTimeOffset? scheduledFor = null,
        Guid? tenantId = null)
    {
        var faker = new Faker<SmsNotification>()
            .CustomInstantiator(f => new SmsNotification(
                    serviceName: serviceName ?? f.Random.Word(),
                    phoneNumber: phoneNumber ?? f.Person.Phone,
                    culture: culture ?? f.PickRandom("pt-BR", "en-US", "es-ES"),
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
