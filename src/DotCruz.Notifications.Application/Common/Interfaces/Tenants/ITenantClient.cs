using DotCruz.Notifications.Application.Common.Interfaces.Tenants.DTOs;

namespace DotCruz.Notifications.Application.Common.Interfaces.Tenants;

public interface ITenantClient
{
    Task<TenantDto?> GetTenantByIdAsync(Guid tenantId, CancellationToken cancellationToken);
}
