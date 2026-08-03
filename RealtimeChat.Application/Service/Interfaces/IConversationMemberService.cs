using RealtimeChat.Application.DTOs.ConversationMembers;
using System;
using System.Collections.Generic;
using System.Text;

namespace RealtimeChat.Application.Service.Interfaces
{
    public interface IConversationMemberService
    {
        Task<bool> AddMemberAsync(string conversationId, AddConversationMemberRequest request);

        Task<bool> RemoveMemberAsync(string conversationId, string userId);

        Task<bool> UpdateRoleAsync(string conversationId, string userId, UpdateConversationMemberRoleRequest request);

        Task<int> CountUnreadMessagesAsync(string conversationId, string userId);

        Task<bool> MarkAsReadAsync(string conversationId, string userId, MarkConversationReadRequest request);

        Task<List<ConversationMemberResponse>> GetMembersAsync(string conversationId);
    }
}
