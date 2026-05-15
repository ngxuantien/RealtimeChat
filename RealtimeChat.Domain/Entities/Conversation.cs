using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using RealtimeChat.Domain.Enums;

namespace RealtimeChat.Domain.Entities;

[BsonIgnoreExtraElements]
public class Conversation : BaseEntity
{
    [BsonElement("type")]
    [BsonRepresentation(BsonType.String)]
    public ConversationType Type { get; set; } = ConversationType.Private;

    [BsonElement("name")]
    public string? Name { get; set; }

    [BsonElement("avatarUrl")]
    public string? AvatarUrl { get; set; }

    [BsonElement("createdBy")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string CreatedBy { get; set; } = string.Empty;

    [BsonElement("lastMessageId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? LastMessageId { get; set; }

    [BsonElement("lastMessagePreview")]
    public string? LastMessagePreview { get; set; }

    [BsonElement("lastMessageAt")]
    public DateTime? LastMessageAt { get; set; }
}