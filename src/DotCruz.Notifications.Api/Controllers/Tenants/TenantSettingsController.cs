using DotCruz.Notifications.Api.Controllers.Base;
using DotCruz.Notifications.Application.DTOs.Base;
using DotCruz.Notifications.Application.UseCases.Tenants.ConfigureTenantBranding;
using DotCruz.Notifications.Application.UseCases.Tenants.ConfigureTenantSmtp;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DotCruz.Notifications.Api.Controllers.Tenants
{
    public class TenantSettingsController(IMediator mediator) : DotCruzNotificationBaseController(mediator)
    {
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ConfigureBranding([FromBody] ConfigureTenantBrandingCommand request, CancellationToken cancellationToken)
        {
            await _mediator.Send(request, cancellationToken);
            return NoContent();
        }

        [HttpPost("smtp")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ConfigureSmtp([FromBody] ConfigureTenantSmtpCommand request, CancellationToken cancellationToken)
        {
            await _mediator.Send(request, cancellationToken);
            return NoContent();
        }
    }
}
