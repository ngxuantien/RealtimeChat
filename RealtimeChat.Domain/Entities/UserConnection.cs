using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace RealtimeChat.Domain.Entities;

[BsonIgnoreExtraElements]
public class UserConnection : BaseEntity
{
    [BsonElement("userId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string UserId { get; set; } = string.Empty;

    [BsonElement("connectionId")]
    public string ConnectionId { get; set; } = string.Empty;

    [BsonElement("deviceId")]
    public string? DeviceId { get; set; }

    [BsonElement("platform")]
    public string Platform { get; set; } = "web";

    [BsonElement("connectedAt")]
    public DateTime ConnectedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("disconnectedAt")]
    public DateTime? DisconnectedAt { get; set; }
}