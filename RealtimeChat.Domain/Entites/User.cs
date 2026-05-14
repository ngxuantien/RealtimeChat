using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using RealtimeChat.Domain.Enums;

namespace RealtimeChat.Domain.Entites;

public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id_User { get; set; } = string.Empty;

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
    public UserStatus Status { get; set; } = UserStatus.Active;

    [BsonElement("isOnline")]
    public bool IsOnline { get; set; }

    [BsonElement("lastSeenAt")]
    public DateTime? LastSeenAt { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public DateTime? UpdatedAt { get; set; }

    [BsonElement("deletedAt")]
    public DateTime? DeletedAt { get; set; }
}
