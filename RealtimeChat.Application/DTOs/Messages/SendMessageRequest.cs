using RealtimeChat.Domain.Entities;
using RealtimeChat.Domain.Enums;

namespace RealtimeChat.Application.DTOs.Messages;

public class SendMessageRequest
{
    public string SenderId { get; set; } = string.Empty;

    public MessageType Type { get; set; } = MessageType.Text;

    public string Content { get; set; } = string.Empty;

    public string? ReplyToMessageId { get; set; }

    public List<MessageAttachment> Attachments { get; set; } = [];
}