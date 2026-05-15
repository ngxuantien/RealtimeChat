using MongoDB.Driver;
using RealtimeChat.Domain.Entities;

namespace RealtimeChat.Infrastructure.Mongo;

public class MongoDbIndexInitializer
{
    private readonly MongoDbContext _context;

    public MongoDbIndexInitializer(MongoDbContext context)
    {
        _context = context;
    }

    public async Task CreateIndexesAsync()
    {
        await CreateUserIndexesAsync();
        await CreateConversationIndexesAsync();
        await CreateConversationMemberIndexesAsync();
        await CreateMessageIndexesAsync();
        await CreateUserConnectionIndexesAsync();
    }

    private async Task CreateUserIndexesAsync()
    {
        var emailIndex = new CreateIndexModel<User>(
            Builders<User>.IndexKeys.Ascending(x => x.Email),
            new CreateIndexOptions { Unique = true });

        await _context.Users.Indexes.CreateOneAsync(emailIndex);
    }

    private async Task CreateConversationIndexesAsync()
    {
        var lastMessageIndex = new CreateIndexModel<Conversation>(
            Builders<Conversation>.IndexKeys.Descending(x => x.LastMessageAt));

        var createdByIndex = new CreateIndexModel<Conversation>(
            Builders<Conversation>.IndexKeys.Ascending(x => x.CreatedBy));

        await _context.Conversations.Indexes.CreateManyAsync([
            lastMessageIndex,
            createdByIndex
        ]);
    }

    private async Task CreateConversationMemberIndexesAsync()
    {
        var uniqueMemberIndex = new CreateIndexModel<ConversationMember>(
            Builders<ConversationMember>.IndexKeys
                .Ascending(x => x.ConversationId)
                .Ascending(x => x.UserId),
            new CreateIndexOptions { Unique = true });

        var userIndex = new CreateIndexModel<ConversationMember>(
            Builders<ConversationMember>.IndexKeys.Ascending(x => x.UserId));

        var conversationIndex = new CreateIndexModel<ConversationMember>(
            Builders<ConversationMember>.IndexKeys.Ascending(x => x.ConversationId));

        await _context.ConversationMembers.Indexes.CreateManyAsync([
            uniqueMemberIndex,
            userIndex,
            conversationIndex
        ]);
    }

    private async Task CreateMessageIndexesAsync()
    {
        var conversationCreatedAtIndex = new CreateIndexModel<Message>(
            Builders<Message>.IndexKeys
                .Ascending(x => x.ConversationId)
                .Descending(x => x.CreatedAt));

        var senderIndex = new CreateIndexModel<Message>(
            Builders<Message>.IndexKeys.Ascending(x => x.SenderId));

        await _context.Messages.Indexes.CreateManyAsync([
            conversationCreatedAtIndex,
            senderIndex
        ]);
    }

    private async Task CreateUserConnectionIndexesAsync()
    {
        var connectionIndex = new CreateIndexModel<UserConnection>(
            Builders<UserConnection>.IndexKeys.Ascending(x => x.ConnectionId),
            new CreateIndexOptions { Unique = true });

        var userIndex = new CreateIndexModel<UserConnection>(
            Builders<UserConnection>.IndexKeys.Ascending(x => x.UserId));

        await _context.UserConnections.Indexes.CreateManyAsync([
            connectionIndex,
            userIndex
        ]);
    }
}