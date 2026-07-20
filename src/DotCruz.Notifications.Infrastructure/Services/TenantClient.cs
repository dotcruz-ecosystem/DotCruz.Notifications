using DotCruz.Notifications.Application.Common.Interfaces.Tenants;
using DotCruz.Notifications.Application.Common.Interfaces.Tenants.DTOs;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DotCruz.Notifications.Infrastructure.Services;

public class TenantClient(HttpClient httpClient) : ITenantClient
{
    private readonly JsonSerializerOptions _serializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        Converters = { new JsonStringEnumConverter() }
    };

    public async Task<TenantDto?> GetTenantByIdAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        var response = await httpClient.GetAsync($"/api/tenants/{tenantId}", cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<TenantDto>(_serializerOptions, cancellationToken);
    }
}
