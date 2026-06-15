using RealtimeChat.Application.DTOs.Messages;
using RealtimeChat.Application.Repositories.Interfaces;
using RealtimeChat.Application.Service.Interfaces;
using RealtimeChat.Domain.Entities;
using RealtimeChat.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace RealtimeChat.Application.Service;

public class MessageService : IMessageService
{
    private readonly IUnitOfWork _unitOfWork;

    public MessageService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public Task<bool> DeleteMessageAsync(string messageId, string userId)
    {
        throw new NotImplementedException();
    }

    public Task<Message?> EditMessageAsync(string messageId, string userId, EditMessageRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<List<Message>> GetMessagesAsync(string conversationId, int page, int pageSize)
    {
        throw new NotImplementedException();
    }

    public async Task<Message?> SendMessageAsync(SendMessageRequest request)
    {
        var conversationRepo = _unitOfWork.GetRepositoryAsync<Conversation>();
        var memberRepo = _unitOfWork.GetRepositoryAsync<ConversationMember>();
        var messageRepo = _unitOfWork.GetRepositoryAsync<Message>();

        var conversation = conversationRepo.GetByIdAsync(request.ConversationId);

        if(conversation == null)
        {
            return null;
        }

        var isMember = memberRepo.FirstOrDefaultAsync(x => 
            x.ConversationId == request.ConversationId && 
            x.UserId == request.SenderId &&
            x.LeftAt == null);

        if (isMember == null)
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(request.ReplyToMessageId))
        {
            var replyMessage = await messageRepo.GetByIdAsync(request.ReplyToMessageId);

            if (replyMessage == null || replyMessage.ConversationId != request.ConversationId)
                return null;
        }

        var message = new Message
        {
            ConversationId = request.ConversationId,
            SenderId = request.SenderId,
            Type = request.Type,
            Content = request.Content,
            ReplyToMessageId = request.ReplyToMessageId,
            Attachments = request.Attachments,
            Status = MessageStatus.Sent,
            CreatedAt = DateTime.UtcNow
        };

        await messageRepo.AddAsync(message);

        return message;
    }
}
