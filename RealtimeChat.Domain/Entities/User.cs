using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using RealtimeChat.Domain.Enums;

namespace RealtimeChat.Domain.Entities;

[BsonIgnoreExtraElements]
public class User : BaseEntity
{
    [BsonElement("displayName")]
    public string DisplayName { get; set; } = string.Empty;

    [BsonElement("email")]
    public string Email { get; set; } = string.Empty;

    [BsonElement("passwordHash")]
    public string PasswordHash { get; set; } = string.Empty;

    [BsonElement("avatarUrl")]
    public string? AvatarUrl { get; set; }

    [BsonElement("bio")]
    public string? Bio { get; set; }

    [BsonElement("status")]
    [BsonRepresentation(BsonType.String)]
    public UserStatus Status { get; set; } = UserStatus.Active;

    [BsonElement("isOnline")]
    public bool IsOnline { get; set; } = false;

    [BsonElement("lastSeenAt")]
    public DateTime? LastSeenAt { get; set; }
}