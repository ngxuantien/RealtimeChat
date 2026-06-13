using System;
using System.Collections.Generic;
using System.Text;

namespace RealtimeChat.Application.DTOs.ConversationMembers;

public class AddConversationMemberRequest
{
    public string UserId { get; set; } = string.Empty;
}
