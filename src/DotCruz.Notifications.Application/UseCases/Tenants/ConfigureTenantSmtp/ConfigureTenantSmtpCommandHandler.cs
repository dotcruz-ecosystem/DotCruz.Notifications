using DotCruz.Notifications.Domain.Interfaces;
using DotCruz.Notifications.Domain.Exceptions.BaseExceptions;
using DotCruz.Notifications.Domain.Exceptions.Resources;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DotCruz.Shared.Security.Context;

namespace DotCruz.Notifications.Application.UseCases.Tenants.ConfigureTenantSmtp
{
    public class ConfigureTenantSmtpCommandHandler : IRequestHandler<ConfigureTenantSmtpCommand>
    {
        private readonly ISmtpConfigService _smtpConfigService;
        private readonly ISecurityContext _securityContext;

        public ConfigureTenantSmtpCommandHandler(ISmtpConfigService smtpConfigService, ISecurityContext securityContext)
        {
            _smtpConfigService = smtpConfigService;
            _securityContext = securityContext;
        }

        public async Task Handle(ConfigureTenantSmtpCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _securityContext.TenantId;
            if (!tenantId.HasValue)
                throw new UnauthorizedException(ResourceMessagesException.TENANT_ID_REQUIRED);

            await _smtpConfigService.SaveAsync(
                tenantId.Value,
                request.Host,
                request.Port,
                request.Username,
                request.Password,
                request.FromName,
                cancellationToken
            );
        }
    }
}
