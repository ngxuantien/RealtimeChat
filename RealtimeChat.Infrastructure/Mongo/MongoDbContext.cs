using Microsoft.Extensions.Options;
using MongoDB.Driver;
using RealtimeChat.Domain.Entities;

namespace RealtimeChat.Infrastructure.Mongo;

public class MongoDbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext(IOptions<MongoDbSettings> options)
    {
        var settings = options.Value;

        var client = new MongoClient(settings.ConnectionString);

        _database = client.GetDatabase(settings.DatabaseName);
    }

    public IMongoCollection<User> Users =>
        _database.GetCollection<User>("users");

    public IMongoCollection<Conversation> Conversations =>
        _database.GetCollection<Conversation>("conversations");

    public IMongoCollection<ConversationMember> ConversationMembers =>
        _database.GetCollection<ConversationMember>("conversationMembers");

    public IMongoCollection<Message> Messages =>
        _database.GetCollection<Message>("messages");

    public IMongoCollection<UserConnection> UserConnections =>
        _database.GetCollection<UserConnection>("userconnections");
}
