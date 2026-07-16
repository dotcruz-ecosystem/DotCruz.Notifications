using Amazon.Scheduler;
using Amazon.Scheduler.Model;
using DotCruz.Notifications.Application.Common.Interfaces.Messaging;
using DotCruz.Notifications.Contracts.Messages.Notifications.SendNotification;
using DotCruz.Notifications.CrossCutting.Settings;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace DotCruz.Notifications.Infrastructure.Services.Messaging;

public class EventBridgeNotificationScheduler : INotificationScheduler
{
    private readonly IAmazonScheduler _schedulerClient;
    private readonly string _queueArn;
    private readonly string _roleArn;

    public EventBridgeNotificationScheduler(IAmazonScheduler schedulerClient, IOptions<AwsSettings> awsSettings)
    {
        _schedulerClient = schedulerClient;
        _queueArn = awsSettings.Value.SqsQueueArn;
        _roleArn = awsSettings.Value.SchedulerRoleArn;
    }

    public async Task ScheduleAsync(SendNotificationMessage payload, DateTimeOffset scheduledFor, CancellationToken cancellationToken = default)
    {
        var scheduleExpression = $"at({scheduledFor.UtcDateTime:yyyy-MM-ddTHH:mm:ss})";

        var request = new CreateScheduleRequest
        {
            Name = $"Notification-{payload.NotificationId}",
            ScheduleExpression = scheduleExpression,
            ScheduleExpressionTimezone = "UTC",
            Target = new Target
            {
                Arn = _queueArn,
                RoleArn = _roleArn,
                Input = JsonSerializer.Serialize(payload)
            },
            ActionAfterCompletion = ActionAfterCompletion.DELETE,
            FlexibleTimeWindow = new FlexibleTimeWindow
            {
                Mode = FlexibleTimeWindowMode.OFF
            }
        };

        await _schedulerClient.CreateScheduleAsync(request, cancellationToken);
    }

    public async Task CancelAsync(Guid notificationId, CancellationToken cancellationToken = default)
    {
        var request = new DeleteScheduleRequest
        {
            Name = $"Notification-{notificationId}"
        };

        try
        {
            await _schedulerClient.DeleteScheduleAsync(request, cancellationToken);
        }
        catch (ResourceNotFoundException)
        {
        }
    }
}
