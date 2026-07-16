using CommonTestUtilities.Entities.Notifications;
using DotCruz.Notifications.Domain.Exceptions.BaseExceptions;
using DotCruz.Notifications.Domain.Exceptions.Resources;
using FluentAssertions;
using Xunit;

namespace Domain.Test.Entities.Notifications;

public class EmailNotificationTest
{
    [Fact]
    public void Success()
    {
        var notification = EmailNotificationBuilder.Build();

        notification.Should().NotBeNull();
        notification.ServiceName.Should().NotBeNullOrWhiteSpace();
        notification.Recipient.Should().NotBeNullOrWhiteSpace();
        notification.Title.Should().NotBeNullOrWhiteSpace();
        notification.Culture.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void Error_ServiceName_Empty()
    {
        var action = () => EmailNotificationBuilder.Build(serviceName: string.Empty);

        action.Should().ThrowExactly<ErrorOnValidationException>()
            .Where(e => e.GetErrorsMessages().Contains(ResourceMessagesException.SERVICE_NAME_EMPTY));
    }

    [Fact]
    public void Error_Recipient_Empty()
    {
        var action = () => EmailNotificationBuilder.Build(recipient: string.Empty);

        action.Should().ThrowExactly<ErrorOnValidationException>()
            .Where(e => e.GetErrorsMessages().Contains(ResourceMessagesException.RECIPIENT_EMPTY));
    }

    [Fact]
    public void Error_Title_Empty()
    {
        var action = () => EmailNotificationBuilder.Build(title: string.Empty);

        action.Should().ThrowExactly<ErrorOnValidationException>()
            .Where(e => e.GetErrorsMessages().Contains(ResourceMessagesException.TITLE_EMPTY));
    }

    [Fact]
    public void Error_Body_And_Template_Empty()
    {
        var action = () => EmailNotificationBuilder.Build(body: string.Empty, templateId: null);

        action.Should().ThrowExactly<ErrorOnValidationException>()
            .Where(e => e.GetErrorsMessages().Contains(ResourceMessagesException.BODY_OR_TEMPLATE_REQUIRED));
    }
}
