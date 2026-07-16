namespace DotCruz.Notifications.Application.Common.Interfaces.Tenants.DTOs;

public sealed record TenantDto(
    Guid Id,
    string Name,
    string Slug,
    string Status,
    TenantAddressDto TenantAddress,
    TenantBrandingDto TenantBranding
)
{
    public string GetFullAddress()
    {
        if (TenantAddress == null)
            return string.Empty;

        return $"{TenantAddress.Street}, {TenantAddress.Number}" +
               (!string.IsNullOrWhiteSpace(TenantAddress.Complement) ? $", {TenantAddress.Complement}" : "") +
               $" - {TenantAddress.Neighborhood}, {TenantAddress.City} - {TenantAddress.State}, CEP {TenantAddress.ZipCode}";
    }
}
