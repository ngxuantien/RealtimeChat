using RealtimeChat.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace RealtimeChat.Application.DTOs.ConversationMembers;

public class UpdateConversationMemberRoleRequest
{
    public ConversationMemberRole Role { get; set; }
}
