using CommonTestUtilities.Entities;
using CommonTestUtilities.Entities.Templates;
using DotCruz.Notifications.Application.Common.Interfaces;
using DotCruz.Notifications.CrossCutting.Settings;
using DotCruz.Notifications.Domain.Entities.Notifications;
using DotCruz.Notifications.Domain.Entities.Templates;
using DotCruz.Notifications.Domain.Enums.Notifications;
using DotCruz.Notifications.Domain.Interfaces;
using DotCruz.Notifications.Domain.Interfaces.Repositories;
using DotCruz.Notifications.Infrastructure.DataAccess;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace WebApi.Test;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    public Guid TenantId { get; } = Guid.NewGuid();
    private Notification _notification = default!;
    private string _apiToken = default!;
    private readonly string _databaseName = "Notifications_Test_" + Guid.NewGuid().ToString("N");
    private string? _databaseConnectionString = null;

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
                var serviceProvider = services.BuildServiceProvider();
                var configuration = serviceProvider.GetRequiredService<IConfiguration>();

                var testApiKey = configuration.GetValue<string>("Settings:ApiKey")!;

                services.Configure<DotCruz.Shared.Security.Authentication.ApiKey.ServiceApiKeyOptions>(
                    DotCruz.Shared.Security.Authentication.ApiKey.ServiceApiKeyDefaults.AuthenticationScheme,
                    options =>
                    {
                        options.Keys["CoreAuth"] = testApiKey;
                    });

                _databaseConnectionString = configuration.GetConnectionString("MongoDb");

                if (string.IsNullOrEmpty(_databaseConnectionString))
                    throw new InvalidOperationException("Could not find connection string for tests");

                RemoveService<IMongoClient>(services);
                RemoveService<IOptions<MongoDbSettings>>(services);
                RemoveService<NotificationDbContext>(services);
                RemoveService<IPublishNotificationService>(services);
                RemoveService<INotificationScheduler>(services);

                var mongoClient = new MongoClient(_databaseConnectionString);
                services.AddSingleton<IMongoClient>(mongoClient);

                services.Configure<MongoDbSettings>(options =>
                {
                    options.DatabaseName = _databaseName;
                });

                services.AddSingleton<NotificationDbContext>();

                var publishServiceMock = new Moq.Mock<IPublishNotificationService>();
                var schedulerMock = new Moq.Mock<INotificationScheduler>();

                services.AddSingleton(publishServiceMock.Object);
                services.AddSingleton(schedulerMock.Object);

                using var scope = services.BuildServiceProvider().CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();

                StartDatabase(dbContext);
                SetApiToken(configuration);
            });
    }

    private static void RemoveService<T>(IServiceCollection services)
    {
        var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(T));
        if (descriptor is not null)
            services.Remove(descriptor);
    }

    public Guid GetTemplateId()
    {
        using var scope = Services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<ITemplateRepository>();
        var template = repo.GetByCodeAsync("CreateUserCommand", "pt-BR", CancellationToken.None).GetAwaiter().GetResult();
        return template?.Id ?? Guid.Empty;
    }

    public string GetTemplateCode() => "CreateUserCommand";
    public string GetApiToken() => _apiToken;

    private void StartDatabase(NotificationDbContext dbContext)
    {
        _notification = NotificationBuilder.Build(NotificationType.Email, tenantId: TenantId);
        dbContext.Notifications.InsertOne(_notification);
    }

    private void SetApiToken(IConfiguration configuration)
        =>  _apiToken = configuration.GetValue<string>("Settings:ApiKey")!;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            var mongoClient = new MongoClient(_databaseConnectionString);
            mongoClient.DropDatabase(_databaseName);
        }

        base.Dispose(disposing);
    }
}
