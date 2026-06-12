using System;
using System.Collections.Generic;
using System.Text;

namespace RealtimeChat.Application.DTOs.Conversations
{
    public class UpdateConversationRequest
    {
        public string? Name { get; set; }

        public string? AvatarUrl { get; set; }
    }
}
