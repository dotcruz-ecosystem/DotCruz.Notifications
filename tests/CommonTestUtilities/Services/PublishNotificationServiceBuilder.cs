using DotCruz.Notifications.Application.Common.Interfaces.Messaging;
using DotCruz.Notifications.Domain.Interfaces;
using Moq;

namespace CommonTestUtilities.Services;

public class PublishNotificationServiceBuilder
{
    private readonly Mock<IPublishNotificationService> _service;

    public PublishNotificationServiceBuilder()
    {
        _service = new Mock<IPublishNotificationService>();
    }

    public IPublishNotificationService Build() => _service.Object;
}