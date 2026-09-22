using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace RealtimeChat.Domain.Entities;

public class MessageReaction
{
    [BsonElement("userId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string UserId { get; set; } = string.Empty;

    [BsonElement("emoji")]
    public string Emoji { get; set; } = string.Empty;
}