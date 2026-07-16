using DotCruz.Notifications.Domain.Entities.Notifications;
using DotCruz.Notifications.Domain.Enums.Notifications;
using DotCruz.Notifications.Domain.Interfaces;
using Moq;

namespace CommonTestUtilities.Factories;

public class NotificationFactoryStrategyBuilder
{
    private readonly Mock<INotificationFactoryStrategy> _strategy;

    public NotificationFactoryStrategyBuilder(NotificationType type)
    {
        _strategy = new Mock<INotificationFactoryStrategy>();
        _strategy.Setup(s => s.Type).Returns(type);
    }

    public NotificationFactoryStrategyBuilder Create(Notification notification)
    {
        _strategy.Setup(s => s.Create(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<Guid?>(),
            It.IsAny<Dictionary<string, object>>(),
            It.IsAny<DateTimeOffset?>(),
            It.IsAny<Guid>()
        )).Returns(notification);

        return this;
    }

    public INotificationFactoryStrategy Build() => _strategy.Object;
}