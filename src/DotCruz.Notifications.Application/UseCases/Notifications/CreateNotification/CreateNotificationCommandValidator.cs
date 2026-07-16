using DotCruz.Notifications.Domain.Exceptions.Resources;
using FluentValidation;

namespace DotCruz.Notifications.Application.UseCases.Notifications.CreateNotification;

public class CreateNotificationCommandValidator : AbstractValidator<CreateNotificationCommand>
{
    public CreateNotificationCommandValidator()
    {
        RuleFor(x => x.Message.Recipient)
            .NotEmpty()
            .WithMessage(ResourceMessagesException.RECIPIENT_EMPTY);

        RuleFor(x => x.Message.Type)
            .IsInEnum()
            .WithMessage(ResourceMessagesException.NOTIFICATION_TYPE_INVALID);

        RuleFor(x => x.Message)
            .Must(m => !string.IsNullOrWhiteSpace(m.Body) || !string.IsNullOrWhiteSpace(m.TemplateCode))
            .WithMessage(ResourceMessagesException.BODY_OR_TEMPLATE_REQUIRED);
    }
}
