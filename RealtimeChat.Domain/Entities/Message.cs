using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using RealtimeChat.Domain.Enums;

namespace RealtimeChat.Domain.Entities;

[BsonIgnoreExtraElements]
public class Message : BaseEntity
{
    [BsonElement("conversationId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string ConversationId { get; set; } = string.Empty;

    [BsonElement("senderId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string SenderId { get; set; } = string.Empty;

    [BsonElement("type")]
    [BsonRepresentation(BsonType.String)]
    public MessageType Type { get; set; } = MessageType.Text;

    [BsonElement("content")]
    public string Content { get; set; } = string.Empty;

    [BsonElement("replyToMessageId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? ReplyToMessageId { get; set; }

    [BsonElement("attachments")]
    public List<MessageAttachment> Attachments { get; set; } = [];

    [BsonElement("status")]
    [BsonRepresentation(BsonType.String)]
    public MessageStatus Status { get; set; } = MessageStatus.Sent;

    [BsonElement("isDeleted")]
    public bool IsDeleted { get; set; }

    [BsonElement("deletedBy")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? DeletedBy { get; set; }

    [BsonElement("editedAt")]
    public DateTime? EditedAt { get; set; }
}