using DotCruz.Notifications.Domain.Entities.Notifications;
using DotCruz.Notifications.Domain.Enums.Notifications;
using DotCruz.Notifications.Domain.Exceptions.BaseExceptions;
using DotCruz.Notifications.Domain.Exceptions.Resources;
using DotCruz.Notifications.Domain.Interfaces.Repositories;
using DotCruz.Shared.Security.Context;
using System.Collections.Concurrent;

namespace CommonTestUtilities.Repositories;

public class InMemoryNotificationStore
{
    public ConcurrentDictionary<Guid, Notification> Notifications { get; } = new();
}

public class InMemoryNotificationRepository : INotificationRepository
{
    private readonly InMemoryNotificationStore _store;
    private readonly ISecurityContext _securityContext;

    public InMemoryNotificationRepository(InMemoryNotificationStore store, ISecurityContext securityContext)
    {
        _store = store;
        _securityContext = securityContext;
    }

    public Task AddAsync(Notification notification, CancellationToken cancellationToken)
    {
        _store.Notifications[notification.Id] = notification;
        return Task.CompletedTask;
    }

    public Task<Notification?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var tenantId = RequireTenantId();

        _store.Notifications.TryGetValue(id, out var notification);

        return Task.FromResult(notification?.TenantId == tenantId ? notification : null);
    }

    public Task<IEnumerable<Notification>> GetPendingScheduledAsync(DateTimeOffset referenceDate, int limit, CancellationToken cancellationToken)
    {
        var tenantId = RequireTenantId();

        var pending = _store.Notifications.Values
            .Where(n => n.Status == NotificationStatus.Pending
                        && n.ScheduledFor != null
                        && n.ScheduledFor <= referenceDate
                        && n.TenantId == tenantId)
            .Take(limit)
            .ToList();

        return Task.FromResult<IEnumerable<Notification>>(pending);
    }

    public Task UpdateAsync(Notification notification, CancellationToken cancellationToken)
    {
        var tenantId = RequireTenantId();

        if (_store.Notifications.TryGetValue(notification.Id, out var existing) && existing.TenantId == tenantId)
            _store.Notifications[notification.Id] = notification;

        return Task.CompletedTask;
    }

    private Guid RequireTenantId()
    {
        var tenantId = _securityContext.TenantId;

        if (!tenantId.HasValue || tenantId.Value == Guid.Empty)
            throw new UnauthorizedException(ResourceMessagesException.TENANT_ID_REQUIRED);

        return tenantId.Value;
    }
}
