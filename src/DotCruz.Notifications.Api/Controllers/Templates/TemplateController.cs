using DotCruz.Notifications.Api.Controllers.Base;
using DotCruz.Notifications.Application.DTOs.Base;
using DotCruz.Notifications.Application.UseCases.Templates.CreateTemplate;
using DotCruz.Notifications.Application.UseCases.Templates.DeleteTemplate;
using DotCruz.Notifications.Application.UseCases.Templates.GetTemplateByCode;
using DotCruz.Notifications.Application.UseCases.Templates.UpdateTemplate;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DotCruz.Notifications.Api.Controllers.Templates;

public class TemplateController : DotCruzNotificationBaseController
{
    public TemplateController(IMediator mediator) : base(mediator) { }

    [HttpGet]
    [Route("{code}/code")]
    [ProducesResponseType(typeof(TemplateResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get([FromRoute] string code, [FromQuery] string? culture, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetTemplateByCodeQuery(code, culture), cancellationToken);
        return Ok(result);
    }}
