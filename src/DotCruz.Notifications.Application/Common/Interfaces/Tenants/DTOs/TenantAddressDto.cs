namespace DotCruz.Notifications.Application.Common.Interfaces.Tenants.DTOs;

public sealed record TenantAddressDto(
    string Street,
    string Number,
    string? Complement,
    string Neighborhood,
    string City,
    string State,
    string ZipCode
)
{
    public string GetFullAddress()
    {
        return $"{Street}, {Number}" +
               (!string.IsNullOrWhiteSpace(Complement) ? $", {Complement}" : "") +
               $" - {Neighborhood}, {City} - {State}, CEP {ZipCode}";
    }
}
