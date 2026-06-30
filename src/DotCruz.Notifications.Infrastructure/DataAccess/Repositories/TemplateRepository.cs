using DotCruz.Notifications.Domain.Entities.Templates;
using DotCruz.Notifications.Domain.Enums.Notifications;
using DotCruz.Notifications.Domain.Interfaces;
using DotCruz.Notifications.Domain.Interfaces.Repositories;
using System.Collections.Concurrent;

namespace DotCruz.Notifications.Infrastructure.DataAccess.Repositories;

public class TemplateRepository : ITemplateRepository
{
    private static readonly ConcurrentDictionary<string, Template> _templatesByCodeAndCulture = new();
    private static readonly ConcurrentDictionary<Guid, Template> _templatesById = new();
    private static readonly object _lock = new();
    private static bool _initialized = false;

    private static readonly List<(string Code, string Culture, string Title)> KnownTemplates = new()
    {
        ("CreateUserCommand", "pt-BR", "Seja bem-vindo!"),
        ("CreateUserCommand", "en", "Welcome!"),
        ("RequestPasswordResetCommand", "pt-BR", "Recuperação de senha"),
        ("RequestPasswordResetCommand", "en", "Password recovery"),
        ("ActivateAccountCommand", "pt-BR", "Ativação de conta"),
        ("ActivateAccountCommand", "en", "Account activation")
    };

    public TemplateRepository(NotificationDbContext context, ITenantProvider tenantProvider)
    {
        InitializeTemplates();
    }

    private static void InitializeTemplates()
    {
        if (_initialized) return;

        lock (_lock)
        {
            if (_initialized) return;

            var assembly = typeof(TemplateRepository).Assembly;

            foreach (var (code, culture, title) in KnownTemplates)
            {
                var cultureFolder = culture.Replace("-", "_");
                var resourceName = $"DotCruz.Notifications.Infrastructure.Templates.Emails.{cultureFolder}.{code}.liquid";

                try
                {
                    using var stream = assembly.GetManifestResourceStream(resourceName);
                    if (stream != null)
                    {
                        using var reader = new StreamReader(stream);
                        var body = reader.ReadToEnd();

                        var template = new Template(
                            code: code,
                            culture: culture,
                            defaultTitle: title,
                            body: body,
                            type: NotificationType.Email,
                            tenantId: Guid.Empty
                        );

                        var key = GetKey(code, culture);
                        _templatesByCodeAndCulture[key] = template;
                        _templatesById[template.Id] = template;
                    }
                }
                catch
                {
                    // Ignora silenciosamente no singleton
                }
            }

            _initialized = true;
        }
    }

    private static string GetKey(string code, string culture) => $"{code.ToLowerInvariant()}:{culture.ToLowerInvariant()}";

    public Task<Template?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        _templatesById.TryGetValue(id, out var template);
        return Task.FromResult(template);
    }

    public Task<Template?> GetByCodeAsync(string code, string culture, CancellationToken cancellationToken)
    {
        var key = GetKey(code, culture);
        _templatesByCodeAndCulture.TryGetValue(key, out var template);
        return Task.FromResult(template);
    }

    public Task<Template?> GetGlobalByCodeAsync(string code, string culture, CancellationToken cancellationToken)
    {
        var key = GetKey(code, culture);
        _templatesByCodeAndCulture.TryGetValue(key, out var template);
        return Task.FromResult(template);
    }
}
