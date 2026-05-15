namespace RealtimeChat.Application.DTOs.Messages;

public class MarkAsReadRequest
{
    public string UserId { get; set; } = string.Empty;

    public string MessageId { get; set; } = string.Empty;
}
