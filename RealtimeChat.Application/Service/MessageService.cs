using RealtimeChat.Application.DTOs.Messages;
using RealtimeChat.Application.Repositories.Interfaces;
using RealtimeChat.Application.Service.Interfaces;
using RealtimeChat.Application.Utils;
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

    public async Task<Message?> DeleteMessageAsync(string messageId, string userId)
    {
        var messageRepo = _unitOfWork.GetRepositoryAsync<Message>();
        var message = await messageRepo.GetByIdAsync(messageId);

        if (message == null) return null;
        if (message.SenderId != userId) return null;
        if (message.IsDeleted) return null;

        message.IsDeleted = true;
        message.UpdatedAt = DateTime.UtcNow;
        message.DeletedBy = userId;

        await messageRepo.UpdateAsync(messageId, message);

        var conversationRepo = _unitOfWork.GetRepositoryAsync<Conversation>();
        var conversation = await conversationRepo.GetByIdAsync(message.ConversationId);

        if(conversation != null && conversation.LastMessageId == message.Id)
        {
            var remaining = await messageRepo.QueryConditionAsync(x =>
            x.ConversationId == message.ConversationId && !x.IsDeleted);

            var newLast = remaining.OrderByDescending(x => x.CreatedAt).FirstOrDefault();

            conversation.LastMessageId = newLast?.Id;
            conversation.LastMessagePreview = newLast != null ? MessagePreviewHelper.Build(newLast) : null;
            conversation.LastMessageAt = newLast?.CreatedAt;
            conversation.UpdatedAt = DateTime.UtcNow;

            await conversationRepo.UpdateAsync(conversation.Id, conversation);
        }

        return message;
    }

    public async Task<Message?> EditMessageAsync(string messageId, string userId, EditMessageRequest request)
    {
        var messageRepo = _unitOfWork.GetRepositoryAsync<Message>();
        var conversationRepo = _unitOfWork.GetRepositoryAsync<Conversation>();
        var message = await messageRepo.GetByIdAsync(messageId);

        if(message == null) return null;
        if(message.SenderId != userId) return null;
        if (message.IsDeleted) return null;
        if (string.IsNullOrWhiteSpace(request.Content)) return null;

        message.Content = request.Content;
        message.UpdatedAt = DateTime.UtcNow;
        message.EditedAt = DateTime.UtcNow;

        await messageRepo.UpdateAsync(messageId, message);

        var conversation = await conversationRepo.GetByIdAsync(message.ConversationId);
        if(conversation != null && conversation.LastMessageId == message.Id)
        {
            conversation.LastMessagePreview = MessagePreviewHelper.Build(message);
            conversation.UpdatedAt = DateTime.UtcNow;
            await conversationRepo.UpdateAsync(conversation.Id, conversation);
        }

        return message;
    }

    public async Task<List<Message>> GetMessagesAsync(string conversationId, int page, int pageSize)
    {
        var messageRepo = _unitOfWork.GetRepositoryAsync<Message>();

        page = page <= 0 ? 1 : page;
        pageSize = pageSize <= 0 ? 20 : pageSize;
        pageSize = pageSize > 100 ? 100 : pageSize;

        var message = await messageRepo.QueryConditionAsync(x =>
            x.ConversationId == conversationId &&
            !x.IsDeleted);

        return message.OrderByDescending(x => x.CreatedAt)
            .Skip((page -1) * pageSize)
            .Take(pageSize)
            .ToList();
    }

    public async Task<Message?> SendMessageAsync(SendMessageRequest request)
    {
        var conversationRepo = _unitOfWork.GetRepositoryAsync<Conversation>();
        var memberRepo = _unitOfWork.GetRepositoryAsync<ConversationMember>();
        var messageRepo = _unitOfWork.GetRepositoryAsync<Message>();

        var conversation = await conversationRepo.GetByIdAsync(request.ConversationId);

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

        conversation.LastMessageId = message.Id;
        conversation.LastMessagePreview = MessagePreviewHelper.Build(message);
        conversation.LastMessageAt = message.CreatedAt;
        conversation.UpdatedAt = DateTime.UtcNow;

        await conversationRepo.UpdateAsync(conversation.Id, conversation);

        return message;
    }

    public async Task<List<Message>> GetAttachmentsAsync(string conversationId)
    {
        var messageRepo = _unitOfWork.GetRepositoryAsync<Message>();

        var message = await messageRepo.QueryConditionAsync(x =>
            x.ConversationId == conversationId &&
            !x.IsDeleted &&
            x.Attachments.Count > 0);

        return message.OrderByDescending(x => x.CreatedAt).ToList();
    }
}
