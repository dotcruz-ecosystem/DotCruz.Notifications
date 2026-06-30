using DotCruz.Notifications.Domain.Entities.Templates;

namespace DotCruz.Notifications.Domain.Interfaces.Repositories;

public interface ITemplateRepository
{
    Task<Template?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Template?> GetByCodeAsync(string code, string culture, CancellationToken cancellationToken);
    Task<Template?> GetGlobalByCodeAsync(string code, string culture, CancellationToken cancellationToken);
}
