using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using RealtimeChat.Domain.Enums;

namespace RealtimeChat.Domain.Entities;

[BsonIgnoreExtraElements]
public class ConversationMember : BaseEntity
{
    [BsonElement("conversationId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string ConversationId { get; set; } = string.Empty;

    [BsonElement("userId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string UserId { get; set; } = string.Empty;

    [BsonElement("role")]
    [BsonRepresentation(BsonType.String)]
    public ConversationMemberRole Role { get; set; } = ConversationMemberRole.Member;

    [BsonElement("nickname")]
    public string? Nickname { get; set; }

    [BsonElement("joinedAt")]
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("leftAt")]
    public DateTime? LeftAt { get; set; }

    [BsonElement("isMuted")]
    public bool IsMuted { get; set; }

    [BsonElement("isPinned")]
    public bool IsPinned { get; set; }

    [BsonElement("lastReadMessageId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? LastReadMessageId { get; set; }

    [BsonElement("lastReadAt")]
    public DateTime? LastReadAt { get; set; }
}