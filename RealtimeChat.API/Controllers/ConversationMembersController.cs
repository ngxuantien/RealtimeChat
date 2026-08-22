using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealtimeChat.Application.DTOs.ConversationMembers;
using RealtimeChat.Application.Service.Interfaces;

namespace RealtimeChat.API.Controllers;

[Authorize]
[ApiController]
[Route("api/conversations/{conversationId}/members")]
public class ConversationMembersController : ControllerBase
{
    private readonly IConversationMemberService _memberService;

    public ConversationMembersController(IConversationMemberService memberService)
    {
        _memberService = memberService;
    }

    [HttpPost]
    public async Task<IActionResult> AddMember(string conversationId, AddConversationMemberRequest request)
    {
        var result = await _memberService.AddMemberAsync(conversationId, request);

        if (!result)
            return BadRequest(new { message = "Cannot add member" });

        return Ok(new { message = "Member added successfully" });
    }

    [HttpDelete("{userId}")]
    public async Task<IActionResult> RemoveMember(string conversationId, string userId)
    {
        var result = await _memberService.RemoveMemberAsync(conversationId, userId);

        if (!result)
            return NotFound(new { message = "Member not found" });

        return Ok(new { message = "Member removed successfully" });
    }

    [HttpPatch("{userId}/role")]
    public async Task<IActionResult> UpdateRole(string conversationId, string userId, UpdateConversationMemberRoleRequest request)
    {
        var result = await _memberService.UpdateRoleAsync(conversationId, userId, request);

        if (!result)
            return NotFound(new { message = "Member not found" });

        return Ok(new { message = "Role updated successfully" });
    }

    [HttpPatch("{userId}/read")]
    public async Task<IActionResult> MarkAsRead(string conversationId, string userId, MarkConversationReadRequest request)
    {
        var result = await _memberService.MarkAsReadAsync(conversationId, userId, request);

        if (!result)
            return NotFound(new { message = "Member not found" });

        return Ok(new { message = "Conversation marked as read" });
    }

    [HttpGet("{userId}/unread-count")]
    public async Task<IActionResult> CountUnreadMessages(string conversationId, string userId)
    {
        var count = await _memberService.CountUnreadMessagesAsync(conversationId, userId);

        return Ok(new { unreadCount = count });
    }

    [HttpGet]
    public async Task<IActionResult> GetMembers(string conversationId)
    {
        var members = await _memberService.GetMembersAsync(conversationId);
        return Ok(members);
    }
}
