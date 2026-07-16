using CommonTestUtilities.Entities.Notifications;
using DotCruz.Notifications.Domain.Exceptions.BaseExceptions;
using DotCruz.Notifications.Domain.Exceptions.Resources;
using FluentAssertions;
using Xunit;

namespace Domain.Test.Entities.Notifications;

public class SmsNotificationTest
{
    [Fact]
    public void Success()
    {
        var notification = SmsNotificationBuilder.Build();

        notification.Should().NotBeNull();
        notification.ServiceName.Should().NotBeNullOrWhiteSpace();
        notification.Recipient.Should().NotBeNullOrWhiteSpace();
        notification.Culture.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void Error_ServiceName_Empty()
    {
        var action = () => SmsNotificationBuilder.Build(serviceName: string.Empty);

        action.Should().ThrowExactly<ErrorOnValidationException>()
            .Where(e => e.GetErrorsMessages().Contains(ResourceMessagesException.SERVICE_NAME_EMPTY));
    }

    [Fact]
    public void Error_PhoneNumber_Empty()
    {
        var action = () => SmsNotificationBuilder.Build(phoneNumber: string.Empty);

        action.Should().ThrowExactly<ErrorOnValidationException>()
            .Where(e => e.GetErrorsMessages().Contains(ResourceMessagesException.RECIPIENT_EMPTY));
    }
}
