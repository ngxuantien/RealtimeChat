using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using RealtimeChat.API.Hubs;
using RealtimeChat.Application.DTOs.Conversations;
using RealtimeChat.Application.Service;
using RealtimeChat.Application.Service.Interfaces;
using RealtimeChat.Domain.Enums;

namespace RealtimeChat.API.Controllers;

[Route("api/conversations")]
public class ConversationsController : BaseApiController
{
    private readonly IConversationService _conversationService;
    private readonly IConversationMemberService _memberService;
    private readonly IFileStorageService _fileStorageService;
    private readonly IHubContext<ChatHub> _hubContext;
    public ConversationsController(IConversationMemberService memberService, IConversationService conversationService, IFileStorageService fileStorageService, IHubContext<ChatHub> hubContext)
    {
        _memberService = memberService;
        _conversationService = conversationService;
        _fileStorageService = fileStorageService;
        _hubContext = hubContext;
    }

    [HttpPost("private")]
    public async Task<IActionResult> CreatePrivateConversation(CreatePrivateConversationRequest request)
    {
        if (request.CurrentUserId != CurrentUserId) return Forbid();

        var conversation = await _conversationService.CreatePrivateConversationAsync(request);
        return Ok(conversation);
    }

    [HttpPost("group")]
    public async Task<IActionResult> CreateGroupConversation(CreateGroupConversationRequest request)
    {
        if(request.CreatedByUserId != CurrentUserId) return Forbid();

        var conversation = await _conversationService.CreateGroupConversationAsync(request);
        return Ok(conversation);
    }

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetUserConversations(string userId)
    {
        if(userId != CurrentUserId) return Forbid();
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
        var members = await _memberService.GetMembersAsync(id);
        if (!members.Any(m => m.UserId == CurrentUserId)) return Forbid();

        var conversation = await _conversationService.UpdateConversationAsync(id, request);

        if (conversation == null)
            return NotFound(new { message = "Conversation not found or not group conversation" });

        return Ok(conversation);
    }

    [HttpPost("{conversationId}/leave/{userId}")]
    public async Task<IActionResult> LeaveConversation(string conversationId, string userId)
    {
        if(userId != CurrentUserId) return Forbid();

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
        if(userId != CurrentUserId) return Forbid();

        var result = await _conversationService.DeleteConversationAsync(conversationId, userId);

        if (!result)
            return NotFound(new { message = "Conversation member not found" });

        return Ok(new { message = "Conversation deleted for user successfully" });
    }

    [HttpDelete("{conversationId}/group")]
    public async Task<IActionResult> DeleteGroup(string conversationId)
    {
        var conversation = await _conversationService.GetConversationByIdAsync(conversationId);
        if(conversation == null)
        {
            return NotFound(new { message = "Conversation not found" });
        }

        if(conversation.Type != ConversationType.Group)
        {
            return BadRequest(new { message = "Only group conversations can be deleted" });
        }

        if (conversation.CreatedBy != CurrentUserId) return Forbid();

        var result = await _conversationService.DeleteGroupAsync(conversationId);
        if(!result) return BadRequest(new { message = "Failed to delete group conversation" });

        return Ok(new {message = "Group conversation deleted successfully" });
    }

    [HttpPost("{id}/avatar")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(6_000_000)]
    public async Task<IActionResult> UpdateGroupAvatar(string id, [FromForm] IFormFile avatar)
    {
        var conversation = await _conversationService.GetConversationByIdAsync(id);
        if (conversation == null) return NotFound(new { message = "Conversation not found" });
        if (conversation.Type != ConversationType.Group)
            return BadRequest(new { message = "Chỉ nhóm mới có thể đổi ảnh đại diện" });

        var members = await _memberService.GetMembersAsync(id);
        if (!members.Any(m => m.UserId == CurrentUserId)) return Forbid();

        if (avatar is null || avatar.Length == 0)
            return BadRequest(new { message = "Vui lòng chọn ảnh đại diện" });

        string avatarUrl;
        try
        {
            await using var stream = avatar.OpenReadStream();
            avatarUrl = await _fileStorageService.SaveAvatarAsync(stream, avatar.ContentType, avatar.Length);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }

        var updated = await _conversationService.UpdateConversationAsync(id, new UpdateConversationRequest { AvatarUrl = avatarUrl });

        var memberGroups = members.Select(m => $"user-{m.UserId}").ToList();
        await _hubContext.Clients.Groups(memberGroups).SendAsync("ConversationInfoUpdated", new
        {
            conversationId = id,
            name = updated!.Name,
            avatarUrl = updated.AvatarUrl,
        });

        return Ok(updated);
    }
}
