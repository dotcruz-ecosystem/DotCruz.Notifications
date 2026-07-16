using CommonTestUtilities.Entities.Notifications;
using DotCruz.Notifications.Domain.Exceptions.BaseExceptions;
using DotCruz.Notifications.Domain.Exceptions.Resources;
using FluentAssertions;
using Xunit;

namespace Domain.Test.Entities.Notifications;

public class PushNotificationTest
{
    [Fact]
    public void Success()
    {
        var notification = PushNotificationBuilder.Build();

        notification.Should().NotBeNull();
        notification.ServiceName.Should().NotBeNullOrWhiteSpace();
        notification.Recipient.Should().NotBeNullOrWhiteSpace();
        notification.Title.Should().NotBeNullOrWhiteSpace();
        notification.Culture.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void Error_ServiceName_Empty()
    {
        var action = () => PushNotificationBuilder.Build(serviceName: string.Empty);

        action.Should().ThrowExactly<ErrorOnValidationException>()
            .Where(e => e.GetErrorsMessages().Contains(ResourceMessagesException.SERVICE_NAME_EMPTY));
    }

    [Fact]
    public void Error_Title_Empty()
    {
        var action = () => PushNotificationBuilder.Build(title: string.Empty);

        action.Should().ThrowExactly<ErrorOnValidationException>()
            .Where(e => e.GetErrorsMessages().Contains(ResourceMessagesException.TITLE_EMPTY));
    }
}
