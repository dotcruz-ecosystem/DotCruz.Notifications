using Amazon.SQS;
using Amazon.SQS.Model;
using DotCruz.Notifications.Application.Common.Interfaces.Messaging;
using DotCruz.Notifications.Contracts.Messages.Notifications.SendNotification;
using DotCruz.Notifications.CrossCutting.Settings;
using DotCruz.Notifications.Domain.Interfaces;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace DotCruz.Notifications.Infrastructure.Services.Messaging;

public class PublishNotificationService : IPublishNotificationService
{
    private readonly IAmazonSQS _sqsClient;
    private readonly string _queueUrl;

    public PublishNotificationService(IAmazonSQS sqsClient, IOptions<AwsSettings> awsSettings)
    {
        _sqsClient = sqsClient;
        _queueUrl = awsSettings.Value.SqsQueueUrl;
    }

    public async Task PublishNotificationCreatedEvent(SendNotificationMessage payload, CancellationToken cancellationToken)
    {
        var request = new SendMessageRequest
        {
            QueueUrl = _queueUrl,
            MessageBody = JsonSerializer.Serialize(payload)
        };

        await _sqsClient.SendMessageAsync(request, cancellationToken);
    }
}
