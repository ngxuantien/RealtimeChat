using System;
using System.Collections.Generic;
using System.Text;

namespace RealtimeChat.Application.DTOs.Conversations
{
    public class CreateGroupConversationRequest
    {
        public string CreatedByUserId { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string? AvatarUrl { get; set; }

        public List<string> MemberIds { get; set; } = [];
    }
}
