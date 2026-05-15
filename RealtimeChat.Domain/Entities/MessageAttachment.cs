using MongoDB.Bson.Serialization.Attributes;

namespace RealtimeChat.Domain.Entities;

public class MessageAttachment
{
    [BsonElement("fileName")]
    public string FileName { get; set; } = string.Empty;

    [BsonElement("fileUrl")]
    public string FileUrl { get; set; } = string.Empty;

    [BsonElement("fileType")]
    public string FileType { get; set; } = string.Empty;

    [BsonElement("fileSize")]
    public long FileSize { get; set; }
}