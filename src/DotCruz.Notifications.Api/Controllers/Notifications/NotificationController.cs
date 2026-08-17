using DotCruz.Notifications.Api.Controllers.Base;
using DotCruz.Notifications.Application.DTOs.Base;
using DotCruz.Notifications.Application.UseCases.Notifications.CreateNotification;
using DotCruz.Notifications.Application.UseCases.Notifications.UpdateNotificationStatus;
using DotCruz.Notifications.Contracts.Messages.Notifications.CreateNotification;
using DotCruz.Notifications.Contracts.Messages.Notifications.UpdateNotificationStatus;
using DotCruz.Shared.Security.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotCruz.Notifications.Api.Controllers.Notifications;

public class NotificationController : DotCruzNotificationBaseController
{
    public NotificationController(IMediator mediator) : base(mediator) { }

    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Post([FromBody] CreateNotificationMessage request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CreateNotificationCommand(request), cancellationToken);

        return Created("", result);
    }

    [HttpPatch("{Id}/status")]
    [Authorize(Policy = SecurityPolicies.ServiceOnly)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Patch(Guid Id, [FromBody] UpdateNotificationStatusRequest request, CancellationToken cancellationToken)
    {
        await _mediator.Send(new UpdateNotificationStatusCommand(Id, request), cancellationToken);
        return NoContent();
    }
}
