using MediatR;

namespace DotCruz.Notifications.Application.UseCases.Tenants.ConfigureTenantBranding;

public record ConfigureTenantBrandingCommand(
    string TenantName,
    string TenantLogoUrl,
    string TenantWebsite,
    string TenantAddress,
    string UnsubscribeUrl,
    string HeaderBackgroundColor = "#ffffff"
) : IRequest;
