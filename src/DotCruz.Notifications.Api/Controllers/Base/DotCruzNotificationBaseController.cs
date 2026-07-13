using DotCruz.Notifications.Application.DTOs.Base;
using DotCruz.Shared.Security.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotCruz.Notifications.Api.Controllers.Base;

[Route("api/[controller]")]
[ApiController]
[Authorize(Policy = SecurityPolicies.ServiceOnly)]
[ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status500InternalServerError)]
public class DotCruzNotificationBaseController(IMediator mediator) : ControllerBase
{
    protected readonly IMediator _mediator = mediator;
}
