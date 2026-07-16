using DotCruz.Notifications.Contracts.Messages.Notifications.SendNotification;

namespace DotCruz.Notifications.Application.Common.Interfaces.Messaging;

public interface INotificationScheduler
{
    Task ScheduleAsync(SendNotificationMessage payload, DateTimeOffset scheduledFor, CancellationToken cancellationToken = default);
    Task CancelAsync(Guid notificationId, CancellationToken cancellationToken = default);
}
