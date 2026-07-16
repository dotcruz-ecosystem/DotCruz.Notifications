using DotCruz.Notifications.Application.Common.Interfaces.Notification;
using DotCruz.Notifications.CrossCutting.Resources;
using DotCruz.Notifications.Domain.Exceptions.BaseExceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DotCruz.Notifications.Application.Common.Behaviors;

public class NotificationFailureBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : INotificationCommand
{
    private readonly ILogger<NotificationFailureBehavior<TRequest, TResponse>> _logger;

    public NotificationFailureBehavior(ILogger<NotificationFailureBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        try
        {
            return await next(cancellationToken);
        }
        catch (NotificationException ex)
        {
            _logger.LogWarning(ex, ResourceLogMessages.ERROR_SENDING_NOTIFICATION, request.NotificationId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ResourceLogMessages.ERROR_SENDING_NOTIFICATION, request.NotificationId);
            throw;
        }
    }
}
