using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using RealtimeChat.Application.Repositories.Interfaces;
using RealtimeChat.Domain.Entities;

namespace RealtimeChat.API.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly IUnitOfWork _unitOfWork;

    public ChatHub(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task JoinConversation(string conversationId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, conversationId);

        await Clients.Group(conversationId).SendAsync(
            "UserJoinedConversation",
            Context.ConnectionId,
            conversationId);
    }

    public async Task LeaveConversation(string conversationId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, conversationId);

        await Clients.Group(conversationId).SendAsync(
            "UserLeftConversation",
            Context.ConnectionId,
            conversationId);
    }

    public async Task Typing(string conversationId, string userId)
    {
        await Clients.Group(conversationId)
            .SendAsync("UserTyping", new
            {
                conversationId,
                userId
            });
    }

    public async Task StopTyping(string conversationId, string userId)
    {
        await Clients
            .Group(conversationId)
            .SendAsync("UserStoppedTyping", new
            {
                conversationId,
                userId
            });
    }

    public async Task MessageSeen(string conversationId, string userId, string messageId)
    {
        await Clients
            .Group(conversationId)
            .SendAsync("MessageSeen", new
            {
                conversationId,
                userId,
                messageId
            });
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");

            var connectionRepo = _unitOfWork.GetRepositoryAsync<UserConnection>();

            await connectionRepo.AddAsync(new UserConnection
            {
                UserId = userId,
                ConnectionId = Context.ConnectionId,
            });

            var activeConnections = await connectionRepo.QueryConditionAsync(
                x => x.UserId == userId && x.DisconnectedAt == null);

            if (activeConnections.Count == 1)
            {
                await SetUserOnlineAsync(userId, true);
            }
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.UserIdentifier;

        if (!string.IsNullOrEmpty(userId))
        {
            var connectionRepo = _unitOfWork.GetRepositoryAsync<UserConnection>();

            var connection = await connectionRepo.FirstOrDefaultAsync(x => x.ConnectionId == Context.ConnectionId);
            if (connection != null)
            {
                connection.DisconnectedAt = DateTime.UtcNow;
                await connectionRepo.UpdateAsync(connection.Id, connection);
            }

            var activeConnections = await connectionRepo.QueryConditionAsync(
                x => x.UserId == userId && x.DisconnectedAt == null);

            if (activeConnections.Count == 0)
            {
                await SetUserOnlineAsync(userId, false);
            }
        }

        await base.OnDisconnectedAsync(exception);
    }

    private async Task SetUserOnlineAsync(string userId, bool isOnline)
    {
        var userRepo = _unitOfWork.GetRepositoryAsync<User>();
        var user = await userRepo.GetByIdAsync(userId);

        if (user == null) return;

        user.IsOnline = isOnline;
        user.LastSeenAt = DateTime.UtcNow;
        await userRepo.UpdateAsync(userId, user);

        await Clients.All.SendAsync("UserOnlineStatusChanged", new
        {
            userId,
            isOnline,
            lastSeenAt = user.LastSeenAt,
        });
    }
}
