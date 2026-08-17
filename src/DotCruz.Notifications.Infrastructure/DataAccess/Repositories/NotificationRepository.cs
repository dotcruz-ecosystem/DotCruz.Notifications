using DotCruz.Notifications.Domain.Entities.Notifications;
using DotCruz.Notifications.Domain.Enums.Notifications;
using DotCruz.Notifications.Domain.Exceptions.BaseExceptions;
using DotCruz.Notifications.Domain.Exceptions.Resources;
using DotCruz.Notifications.Domain.Interfaces.Repositories;
using DotCruz.Shared.Security.Context;
using MongoDB.Driver;

namespace DotCruz.Notifications.Infrastructure.DataAccess.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly NotificationDbContext _context;
    private readonly ISecurityContext _securityContext;

    public NotificationRepository(NotificationDbContext context, ISecurityContext securityContext)
    {
        _context = context;
        _securityContext = securityContext;
    }

    public async Task AddAsync(Notification notification, CancellationToken cancellationToken)
    {
        await _context.Notifications.InsertOneAsync(notification, cancellationToken: cancellationToken);
    }

    public async Task<Notification?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var tenantId = RequireTenantId();

        return await _context.Notifications
            .Find(n => n.Id == id && n.TenantId == tenantId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<Notification>> GetPendingScheduledAsync(DateTimeOffset referenceDate, int limit, CancellationToken cancellationToken)
    {
        var tenantId = RequireTenantId();

        return await _context.Notifications
            .Find(n => n.Status == NotificationStatus.Pending &&
                       n.ScheduledFor != null &&
                       n.ScheduledFor <= referenceDate &&
                       n.TenantId == tenantId)
            .Limit(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task UpdateAsync(Notification notification, CancellationToken cancellationToken)
    {
        var tenantId = RequireTenantId();

        await _context.Notifications.ReplaceOneAsync(
            n => n.Id == notification.Id && n.TenantId == tenantId,
            notification,
            cancellationToken: cancellationToken);
    }

    private Guid RequireTenantId()
    {
        var tenantId = _securityContext.TenantId;

        if (!tenantId.HasValue || tenantId.Value == Guid.Empty)
            throw new UnauthorizedException(ResourceMessagesException.TENANT_ID_REQUIRED);

        return tenantId.Value;
    }
}
