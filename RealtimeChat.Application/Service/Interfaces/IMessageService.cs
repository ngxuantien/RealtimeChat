using RealtimeChat.Application.DTOs.Messages;
using RealtimeChat.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace RealtimeChat.Application.Service.Interfaces;

public interface IMessageService
{
    Task<Message?> SendMessageAsync(SendMessageRequest request);

    Task<List<Message>> GetMessagesAsync(string conversationId, int page, int pageSize);

    Task<Message?> EditMessageAsync(string messageId, string userId, EditMessageRequest request);

    Task<bool> DeleteMessageAsync(string messageId, string userId);
}
