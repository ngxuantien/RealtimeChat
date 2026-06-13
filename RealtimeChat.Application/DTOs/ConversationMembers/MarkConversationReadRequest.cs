using System;
using System.Collections.Generic;
using System.Text;

namespace RealtimeChat.Application.DTOs.ConversationMembers;

public class MarkConversationReadRequest
{
    public string MessageId { get; set; } = string.Empty;
}
