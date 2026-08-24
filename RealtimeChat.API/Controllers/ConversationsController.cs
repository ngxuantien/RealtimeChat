using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealtimeChat.Application.DTOs.Conversations;
using RealtimeChat.Application.Service.Interfaces;

namespace RealtimeChat.API.Controllers;

[Authorize]
[ApiController]
[Route("api/conversations")]
public class ConversationsController : ControllerBase
{
    private readonly IConversationService _conversationService;
    public ConversationsController(IConversationService conversationService)
    {
        _conversationService = conversationService;
    }

    [HttpPost("private")]
    public async Task<IActionResult> CreatePrivateConversation(CreatePrivateConversationRequest request)
    {
        var conversation = await _conversationService.CreatePrivateConversationAsync(request);
        return Ok(conversation);
    }

    [HttpPost("group")]
    public async Task<IActionResult> CreateGroupConversation(CreateGroupConversationRequest request)
    {
        var conversation = await _conversationService.CreateGroupConversationAsync(request);
        return Ok(conversation);
    }

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetUserConversations(string userId)
    {
        var conversations = await _conversationService.GetUserConversationAsync(userId);

        return Ok(conversations);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetConversationById(string id)
    {
        var conversation = await _conversationService.GetConversationByIdAsync(id);

        if (conversation == null)
            return NotFound(new { message = "Conversation not found" });

        return Ok(conversation);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateConversation(string id, UpdateConversationRequest request)
    {
        var conversation = await _conversationService.UpdateConversationAsync(id, request);

        if (conversation == null)
            return NotFound(new 
            { 
                message = "Conversation not found or not group conversation" 
            });

        return Ok(conversation);
    }

    [HttpPost("{conversationId}/leave/{userId}")]
    public async Task<IActionResult> LeaveConversation(string conversationId, string userId)
    {
        var result = await _conversationService.LeaveConversationAsync(conversationId, userId);

        if (!result)
            return NotFound(new 
            { 
                message = "Conversation member not found" 
            });

        return Ok(new 
            { 
                message = "Left conversation successfully" 
            });
    }

    [HttpDelete("{conversationId}/user/{userId}")]
    public async Task<IActionResult> DeleteConversationForUser(string conversationId, string userId)
    {
        var result = await _conversationService.DeleteConversationAsync(conversationId, userId);

        if (!result)
            return NotFound(new { message = "Conversation member not found" });

        return Ok(new { message = "Conversation deleted for user successfully" });
    }
}
