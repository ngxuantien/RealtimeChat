using RealtimeChat.Application.DTOs.Conversations;
using RealtimeChat.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace RealtimeChat.Application.Service.Interfaces;

public interface IConversationService
{
    Task<Conversation> CreatePrivateConversationAsync(CreatePrivateConversationRequest request);

    Task<Conversation> CreateGroupConversationAsync(CreateGroupConversationRequest request);

    Task<List<Conversation>> GetUserConversationAsync(string userId);

    Task<Conversation?> GetConversationByIdAsync(string id);

    Task<Conversation?> UpdateConversationAsync(string id, UpdateConversationRequest request);

    Task<bool> LeaveConversationAsync(string conversationId, string userId);

    Task<bool> DeleteConversationAsync(string conversationId, string userId);
}
