using DotCruz.Notifications.Contracts.Messages.Notifications.SendNotification;
using DotCruz.Notifications.Domain.Entities.Notifications;

namespace DotCruz.Notifications.Application.Common.Interfaces.Messaging;

public interface IPublishNotificationService
{
    Task PublishNotificationCreatedEvent(SendNotificationMessage payload, CancellationToken cancellationToken);
}
