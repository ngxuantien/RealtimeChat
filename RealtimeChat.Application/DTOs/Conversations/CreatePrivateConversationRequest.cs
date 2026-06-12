namespace RealtimeChat.Application.DTOs.Conversations;

public class CreatePrivateConversationRequest
{
    public string CurrentUserId { get; set; } = string.Empty;

    public string TargetUserId { get; set; } = string.Empty;
}
