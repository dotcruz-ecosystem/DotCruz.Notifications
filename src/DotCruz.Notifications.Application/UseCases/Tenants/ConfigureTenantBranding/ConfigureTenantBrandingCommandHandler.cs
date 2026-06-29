using DotCruz.Notifications.Application.Common.Utils;
using DotCruz.Notifications.Domain.Entities.Tenants;
using DotCruz.Notifications.Domain.Exceptions.BaseExceptions;
using DotCruz.Notifications.Domain.Exceptions.Resources;
using DotCruz.Notifications.Domain.Interfaces;
using DotCruz.Notifications.Domain.Interfaces.Repositories;
using MediatR;

namespace DotCruz.Notifications.Application.UseCases.Tenants.ConfigureTenantBranding;

public class ConfigureTenantBrandingCommandHandler : IRequestHandler<ConfigureTenantBrandingCommand>
{
    private readonly ITenantSettingsRepository _tenantSettingsRepository;
    private readonly ITenantProvider _tenantProvider;

    public ConfigureTenantBrandingCommandHandler(
        ITenantSettingsRepository tenantSettingsRepository,
        ITenantProvider tenantProvider)
    {
        _tenantSettingsRepository = tenantSettingsRepository;
        _tenantProvider = tenantProvider;
    }

    public async Task Handle(ConfigureTenantBrandingCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.TenantId;
        if (!tenantId.HasValue)
            throw new UnauthorizedException(ResourceMessagesException.TENANT_ID_REQUIRED);

        var (headerHtml, footerHtml) = EmailBrandingBuilder.Build(
            request.TenantName,
            request.TenantLogoUrl,
            request.TenantWebsite,
            request.TenantAddress,
            request.UnsubscribeUrl,
            request.HeaderBackgroundColor
        );

        var settings = await _tenantSettingsRepository.GetByTenantIdAsync(tenantId.Value, cancellationToken);

        if (settings == null)
        {
            settings = new TenantSettings(tenantId.Value, headerHtml, footerHtml);
            await _tenantSettingsRepository.AddAsync(settings, cancellationToken);
        }
        else
        {
            settings.UpdateBranding(headerHtml, footerHtml);
            await _tenantSettingsRepository.UpdateAsync(settings, cancellationToken);
        }
    }
}
