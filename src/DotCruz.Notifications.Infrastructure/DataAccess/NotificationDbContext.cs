using DotCruz.Notifications.Domain.Entities.Templates;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using DotCruz.Notifications.Domain.Entities.Notifications;
using DotCruz.Notifications.CrossCutting.Settings;

namespace DotCruz.Notifications.Infrastructure.DataAccess;

public class NotificationDbContext
{
    private readonly IMongoDatabase _database;

    public NotificationDbContext(IMongoClient mongoClient, IOptions<MongoDbSettings> settings)
    {
        _database = mongoClient.GetDatabase(settings.Value.DatabaseName);
    }

    public IMongoCollection<Notification> Notifications => _database.GetCollection<Notification>("notifications");
    public IMongoCollection<Template> Templates => _database.GetCollection<Template>("templates");

    public async Task CreateIndexesAsync()
    {
        var notificationIndexes = Notifications.Indexes;
        
        var statusIndex = new CreateIndexModel<Notification>(
            Builders<Notification>.IndexKeys.Ascending(x => x.Status).Descending(x => x.CreatedAt),
            new CreateIndexOptions { Background = true }
        );

        await notificationIndexes.CreateOneAsync(statusIndex);
    }
}
