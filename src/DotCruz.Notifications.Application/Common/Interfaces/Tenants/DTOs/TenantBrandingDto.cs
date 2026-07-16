namespace DotCruz.Notifications.Application.Common.Interfaces.Tenants.DTOs;

public sealed record TenantBrandingDto(
    string LogoUrl,
    string HeaderBackgroundColor,
    string Website,
    string UnsubscribeUrl
);
