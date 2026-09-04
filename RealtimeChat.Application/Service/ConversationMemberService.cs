using RealtimeChat.Application.DTOs.ConversationMembers;
using RealtimeChat.Application.Repositories.Interfaces;
using RealtimeChat.Application.Service.Interfaces;
using RealtimeChat.Domain.Entities;
using RealtimeChat.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using static MongoDB.Bson.Serialization.Serializers.SerializerHelper;

namespace RealtimeChat.Application.Service
{
    public class ConversationMemberService : IConversationMemberService
    {
        private readonly IUnitOfWork _unitOfWork;
        public ConversationMemberService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> AddMemberAsync(string conversationId, AddConversationMemberRequest request)
        {
            var conversationRepo = _unitOfWork.GetRepositoryAsync<Conversation>();
            var memberRepo = _unitOfWork.GetRepositoryAsync<ConversationMember>();

            var conversation = await conversationRepo.GetByIdAsync(conversationId);

            if(conversation == null || conversation.Type != ConversationType.Group)
            {
                return false;
            }

            var user = await memberRepo.FirstOrDefaultAsync(x =>
                x.ConversationId == conversationId &&
                x.UserId == request.UserId &&
                x.LeftAt == null);

            if (user != null)
                return false;

            var member = new ConversationMember
            {
                ConversationId = conversationId,
                UserId = request.UserId,
                Role = ConversationMemberRole.Member,
                JoinedAt = DateTime.UtcNow
            };

            await memberRepo.AddAsync(member);

            return true;
        }

        public async Task<int> CountUnreadMessagesAsync(string conversationId, string userId)
        {
            var memberRepo = _unitOfWork.GetRepositoryAsync<ConversationMember>();
            var messageRepo = _unitOfWork.GetRepositoryAsync<Message>();

            var member = await memberRepo.FirstOrDefaultAsync(x =>
                x.ConversationId == conversationId &&
                x.UserId == userId &&
                x.LeftAt == null);

            if (member == null)
                return 0;

            if (string.IsNullOrWhiteSpace(member.LastReadMessageId))
            {
                var allMessages = await messageRepo.QueryConditionAsync(x =>
                    x.ConversationId == conversationId &&
                    x.SenderId != userId &&
                    x.DeletedAt == null);

                return allMessages.Count;
            }

            var lastReadMessage = await messageRepo.GetByIdAsync(member.LastReadMessageId);

            if (lastReadMessage == null)
                return 0;

            var unreadMessages = await messageRepo.QueryConditionAsync(x =>
                x.ConversationId == conversationId &&
                x.SenderId != userId &&
                x.CreatedAt > lastReadMessage.CreatedAt &&
                x.DeletedAt == null);

            return unreadMessages.Count;
        }

        public async Task<bool> MarkAsReadAsync(string conversationId, string userId, MarkConversationReadRequest request)
        {
            var memberRepo = _unitOfWork.GetRepositoryAsync<ConversationMember>();

            var member = await memberRepo.FirstOrDefaultAsync(x =>
                x.ConversationId == conversationId &&
                x.UserId == userId &&
                x.LeftAt == null);

            if (member == null)
                return false;

            member.LastReadMessageId = request.MessageId;
            member.UpdatedAt = DateTime.UtcNow;

            await memberRepo.UpdateAsync(member.Id, member);

            return true;
        }

        public async Task<bool> RemoveMemberAsync(string conversationId, string userId)
        {
            var memberRepo = _unitOfWork.GetRepositoryAsync<ConversationMember>();

            var user = await memberRepo.FirstOrDefaultAsync(x =>
                x.ConversationId == conversationId &&
                x.UserId == userId &&
                x.LeftAt == null);

            if (user == null)
                return false;

            user.LeftAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;

            await memberRepo.UpdateAsync(user.Id, user);
            return true;
        }

        public async Task<bool> UpdateRoleAsync(string conversationId, string userId, UpdateConversationMemberRoleRequest request)
        {
            var memberRepo = _unitOfWork.GetRepositoryAsync<ConversationMember>();

            var member = await memberRepo.FirstOrDefaultAsync(x =>
                x.ConversationId == conversationId &&
                x.UserId == userId &&
                x.LeftAt == null);

            if (member == null)
                return false;

            member.Role = request.Role;
            member.UpdatedAt = DateTime.UtcNow;

            await memberRepo.UpdateAsync(member.Id, member);

            return true;
        }

        public async Task<List<ConversationMemberResponse>> GetMembersAsync(string conversationId)
        {
            var memberRepo = _unitOfWork.GetRepositoryAsync<ConversationMember>();
            var userRepo = _unitOfWork.GetRepositoryAsync<User>();

            var members = await memberRepo.QueryConditionAsync(x =>
                x.ConversationId == conversationId &&
                x.LeftAt == null);

            var result = new List<ConversationMemberResponse>();

            foreach (var member in members)
            {
                var user = await userRepo.GetByIdAsync(member.UserId);

                if (user == null)
                    continue;

                result.Add(new ConversationMemberResponse
                {
                    UserId = user.Id,
                    DisplayName = user.DisplayName,
                    AvatarUrl = user.AvatarUrl,
                    IsOnline = user.IsOnline,
                    Role = member.Role.ToString(),
                    IsPinned = member.IsPinned,
                    IsMuted = member.IsMuted,
                });
            }

            return result;
        }

        public async Task<bool> UpdateMuteAsync(string conversationId, string userId, bool isMuted)
        {
            var memberRepo = _unitOfWork.GetRepositoryAsync<ConversationMember>();
            var member = await memberRepo.FirstOrDefaultAsync(x =>
                x.ConversationId == conversationId &&
                x.UserId == userId &&
                x.LeftAt == null);

            if (member == null) return false;

            member.IsMuted = isMuted;
            await memberRepo.UpdateAsync(member.Id, member);

            return true;
        }
    }
}
