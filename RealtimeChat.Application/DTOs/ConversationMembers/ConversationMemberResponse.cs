using System;
using System.Collections.Generic;
using System.Text;

namespace RealtimeChat.Application.DTOs.ConversationMembers;

public class ConversationMemberResponse
{
    public string UserId { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string? AvatarUrl { get; set; }

    public bool IsOnline { get; set; }

    public string Role { get; set; } = string.Empty;

    public bool IsPinned { get; set; }

    public bool IsMuted { get; set; }

    public string? LastReadMessageId { get; set; }
}
