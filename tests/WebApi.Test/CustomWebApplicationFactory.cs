using CommonTestUtilities.Entities;
using CommonTestUtilities.Repositories;
using DotCruz.Notifications.Application.Common.Interfaces.Messaging;
using DotCruz.Notifications.Application.Common.Interfaces.Tenants;
using DotCruz.Notifications.Domain.Enums.Notifications;
using DotCruz.Notifications.Domain.Interfaces;
using DotCruz.Notifications.Domain.Interfaces.Repositories;
using DotCruz.Notifications.Infrastructure.DataAccess;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace WebApi.Test;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    public Guid TenantId { get; } = Guid.NewGuid();

    private readonly InMemoryNotificationStore _store = new();
    private string _apiToken = default!;

    public CustomWebApplicationFactory()
    {
        Environment.SetEnvironmentVariable("Settings__Jwt__Issuer", "test-issuer");
        Environment.SetEnvironmentVariable("Settings__Jwt__Audience", "test-audience");
        Environment.SetEnvironmentVariable("Settings__Jwt__JwksUrl", "https://localhost:8080/.well-known/jwks.json");
        Environment.SetEnvironmentVariable("Settings__ServiceAuth__Self__Name", "Notifications");
        Environment.SetEnvironmentVariable("Settings__ServiceAuth__Self__Key", "test-service-key");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test")
            .ConfigureServices(services =>
            {
                using var serviceProvider = services.BuildServiceProvider();
                var configuration = serviceProvider.GetRequiredService<IConfiguration>();

                var testApiKey = configuration.GetValue<string>("Settings:ApiKey")!;

                services.Configure<DotCruz.Shared.Security.Authentication.ApiKey.ServiceApiKeyOptions>(
                    DotCruz.Shared.Security.Authentication.ApiKey.ServiceApiKeyDefaults.AuthenticationScheme,
                    options =>
                    {
                        options.Keys["CoreAuth"] = testApiKey;
                    });

                RemoveService<IMongoClient>(services);
                RemoveService<NotificationDbContext>(services);
                RemoveService<INotificationRepository>(services);
                RemoveService<IPublishNotificationService>(services);
                RemoveService<INotificationScheduler>(services);
                RemoveService<ITenantClient>(services);

                services.AddSingleton(_store);
                services.AddScoped<INotificationRepository, InMemoryNotificationRepository>();

                services.AddSingleton(new Moq.Mock<IPublishNotificationService>().Object);
                services.AddSingleton(new Moq.Mock<INotificationScheduler>().Object);
                services.AddSingleton(new Moq.Mock<ITenantClient>().Object);

                SeedNotifications();
                SetApiToken(configuration);
            });
    }

    private static void RemoveService<T>(IServiceCollection services)
    {
        var descriptors = services.Where(d => d.ServiceType == typeof(T)).ToList();

        foreach (var descriptor in descriptors)
            services.Remove(descriptor);
    }

    public Guid GetTemplateId()
    {
        using var scope = Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ITemplateRepository>();
        var template = repository.GetByCodeAsync("CreateUserCommand", "pt-BR", CancellationToken.None)
            .GetAwaiter()
            .GetResult();

        return template?.Id ?? Guid.Empty;
    }

    public string GetTemplateCode() => "CreateUserCommand";

    public string GetApiToken() => _apiToken;

    private void SeedNotifications()
    {
        var notification = NotificationBuilder.Build(NotificationType.Email, tenantId: TenantId);
        _store.Notifications[notification.Id] = notification;
    }

    private void SetApiToken(IConfiguration configuration)
        => _apiToken = configuration.GetValue<string>("Settings:ApiKey")!;
}
