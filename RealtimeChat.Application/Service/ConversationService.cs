using RealtimeChat.Application.DTOs.Conversations;
using RealtimeChat.Application.Repositories.Interfaces;
using RealtimeChat.Application.Service.Interfaces;
using RealtimeChat.Domain.Entities;
using RealtimeChat.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace RealtimeChat.Application.Service
{
    public class ConversationService : IConversationService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ConversationService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Conversation> CreatePrivateConversationAsync(CreatePrivateConversationRequest request)
        {
            var conversationRepo = _unitOfWork.GetRepositoryAsync<Conversation>();
            var memberRepo = _unitOfWork.GetRepositoryAsync<ConversationMember>();

            var exitingMembers = await memberRepo.QueryConditionAsync(x => 
                x.LeftAt == null &&
                (x.UserId == request.CurrentUserId || x.UserId == request.TargetUserId));

            var conversationIds = exitingMembers
                .GroupBy(x => x.ConversationId)
                .Where(g => g.Count() == 2)
                .Select(g => g.Key)
                .ToList();

            var exitingPrivate = await conversationRepo.FirstOrDefaultAsync(x =>
                conversationIds.Contains(x.Id) &&
                x.Type == ConversationType.Private);

            if(exitingPrivate != null)
            {
                return exitingPrivate;
            }

            var conversation = new Conversation
            {
                Type = ConversationType.Private,
                CreatedBy = request.CurrentUserId,
                CreatedAt = DateTime.UtcNow
            };

            await conversationRepo.AddAsync(conversation);

            await memberRepo.AddAsync(new List<ConversationMember>
            {
                new()
                {
                    ConversationId = conversation.Id,
                    UserId = request.CurrentUserId,
                },
                new()
                {
                    ConversationId = conversation.Id,
                    UserId = request.TargetUserId,
                }
            });

            return conversation;
        }

        public async Task<Conversation> CreateGroupConversationAsync(CreateGroupConversationRequest request)
        {
            var conversationRepo = _unitOfWork.GetRepositoryAsync<Conversation>();
            var memberRepo = _unitOfWork.GetRepositoryAsync<ConversationMember>();

            var conversation = new Conversation
            {
                Type = ConversationType.Group,
                Name = request.Name,
                AvatarUrl = request.AvatarUrl,
                CreatedBy = request.CreatedByUserId,
                CreatedAt = DateTime.UtcNow,
            };

            await conversationRepo.AddAsync(conversation);

            var memberIds = request.MemberIds
                .Append(request.CreatedByUserId)
                .Distinct()
                .ToList();

            var members = memberIds.Select(userId => new ConversationMember
            {
                ConversationId = conversation.Id,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
            });

            await memberRepo.AddAsync(members);

            return conversation;
        }

        public async Task<bool> DeleteConversationAsync(string conversationId, string userId)
        {
            return await LeaveConversationAsync(conversationId, userId);
        }

        public async Task<Conversation?> GetConversationByIdAsync(string id)
        {
            return await _unitOfWork.GetRepositoryAsync<Conversation>().FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<List<Conversation>> GetUserConversationAsync(string userId)
        {
            var conversationRepo = _unitOfWork.GetRepositoryAsync<Conversation>();
            var memberRepo = _unitOfWork.GetRepositoryAsync<ConversationMember>();

            var members = await memberRepo.QueryConditionAsync(x =>
                x.UserId == userId &&
                x.LeftAt == null);

            var conversationIds = members.Select(x => x.ConversationId).ToList();

            var conversations = await conversationRepo.QueryConditionAsync(x =>
                conversationIds.Contains(x.Id) && x.DeletedAt == null);

            return conversations.ToList();
        }

        public async Task<bool> LeaveConversationAsync(string conversationId, string userId)
        {
            var memberRepo = _unitOfWork.GetRepositoryAsync<ConversationMember>();

            var member = await memberRepo.FirstOrDefaultAsync(x =>
                x.ConversationId == conversationId &&
                x.UserId == userId &&
                x.LeftAt == null);

            if (member == null) 
            { 
                return false; 
            }

            member.LeftAt = DateTime.UtcNow;
            member.UpdatedAt = DateTime.UtcNow;

            return true;
        }

        public async Task<Conversation?> UpdateConversationAsync(string id, UpdateConversationRequest request)
        {
            var repository = _unitOfWork.GetRepositoryAsync<Conversation>();
            var conversation = await repository.GetByIdAsync(id);

            if(conversation == null)
                return null;

            if (conversation.Type != ConversationType.Group)
                return null;

            if (!string.IsNullOrWhiteSpace(request.Name))
                conversation.Name = request.Name;

            if (!string.IsNullOrWhiteSpace(request.AvatarUrl))
                conversation.AvatarUrl = request.AvatarUrl;

            conversation.UpdatedAt = DateTime.UtcNow;

            await repository.UpdateAsync(id, conversation);

            return conversation;
        }
    }
}
