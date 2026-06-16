using Microsoft.AspNetCore.SignalR;

namespace RealtimeChat.API.Hubs;

public class ChatHub : Hub
{
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
        await Clients.All.SendAsync("UserConnected", Context.ConnectionId);

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await Clients.All.SendAsync("UserDisconnected", Context.ConnectionId);

        await base.OnDisconnectedAsync(exception);
    }
}
