using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using RealtimeChat.API.Hubs;
using RealtimeChat.Application.DTOs.Messages;
using RealtimeChat.Application.Service.Interfaces;
using RealtimeChat.Domain.Entities;

namespace RealtimeChat.API.Controllers;

[ApiController]
[Route("api/messages")]
public class MessagesController : ControllerBase
{
    private readonly IMessageService _messageService;
    private readonly IHubContext<ChatHub> _hubContext;

    public MessagesController(IMessageService messageService, IHubContext<ChatHub> hubContext)
    {
        _messageService = messageService;
        _hubContext = hubContext;
    }

    [HttpPost]
    public async Task<IActionResult> SendMessage(SendMessageRequest request)
    {
        var result = await _messageService.SendMessageAsync(request);
        if(result == null)
        {
            return BadRequest(new
            {
                result = "Cannot send message",
            });
        }

        await _hubContext
            .Clients
            .Group(result.ConversationId)
            .SendAsync("ReceiveMessage", result);


        return Ok(result);
    }

    [HttpGet("conversation/{conversationId}")]
    public async Task<IActionResult> GetMessages(string conversationId, int page = 1, int pageSize = 20)
    {
        var result = await _messageService.GetMessagesAsync(conversationId, page, pageSize);
        return Ok(result);
    }

    [HttpPut("{messageId}/user/{userId}")]
    public async Task<IActionResult> UpdateMessage(string messageId, string userId, [FromBody] EditMessageRequest request)
    {
        var result = await _messageService.EditMessageAsync(messageId, userId, request);
        if (result == null)
        {
            return BadRequest(new
            {
                message = "Cannot edit message",
            });
        }

        await _hubContext
            .Clients
            .Group(result.ConversationId)
            .SendAsync("MessageEdited", result);

        return Ok(result);
    }

    [HttpDelete("{messageId}/user/{userId}")]
    public async Task<IActionResult> DeleteMessage(string messageId, string userId)
    {
        var result = await _messageService.DeleteMessageAsync(messageId, userId);
        if (!result)
        {
            return BadRequest(new
            {
                message = "Cannot delete message",
            });
        }

        return Ok(new
        {
            message = "Message deleted successfully"
        });
    }
}
