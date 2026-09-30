using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using RealtimeChat.API.Hubs;
using RealtimeChat.Application.DTOs.ConversationMembers;
using RealtimeChat.Application.Service.Interfaces;
using RealtimeChat.Domain.Enums;

namespace RealtimeChat.API.Controllers;

[Route("api/conversations/{conversationId}/members")]
public class ConversationMembersController : BaseApiController
{
    private readonly IConversationMemberService _memberService;
    private readonly IConversationService _conversationService;
    private readonly IHubContext<ChatHub> _hubContext;

    public ConversationMembersController(IConversationMemberService memberService, IConversationService conversationService, IHubContext<ChatHub> hubContext)
    {
        _memberService = memberService;
        _conversationService = conversationService;
        _hubContext = hubContext;
    }

    [HttpPatch("{userId}/role")]
    public async Task<IActionResult> UpdateRole(string conversationId, string userId, UpdateConversationMemberRoleRequest request)
    {
        var conversation = await _conversationService.GetConversationByIdAsync(conversationId);
        if (conversation == null) return NotFound(new { message = "Không tìm thấy cuộc trò chuyện" });

        var isAdmin = await _memberService.IsMemberWithRoleAsync(conversationId, CurrentUserId, ConversationMemberRole.Admin);
        if (conversation.CreatedBy != CurrentUserId && !isAdmin) return Forbid();

        var result = await _memberService.UpdateRoleAsync(conversationId, userId, request);

        if (!result)
            return NotFound(new { message = "Không tìm thấy thành viên" });

        return Ok(new { message = "Cập nhật vai trò thành công" });
    }

    [HttpPatch("{userId}/read")]
    public async Task<IActionResult> MarkAsRead(string conversationId, string userId, MarkConversationReadRequest request)
    {
        if (userId != CurrentUserId) return Forbid();

        var result = await _memberService.MarkAsReadAsync(conversationId, userId, request);

        if (!result)
            return NotFound(new { message = "Không tìm thấy thành viên" });

        await _hubContext.Clients.Group(conversationId).SendAsync("MessageRead", new
        {
            conversationId,
            userId,
            lastReadMessageId = request.MessageId,
        });

        return Ok(new { message = "Đã đánh dấu đã đọc" });
    }

    [HttpGet("{userId}/unread-count")]
    public async Task<IActionResult> CountUnreadMessages(string conversationId, string userId)
    {
        if (userId != CurrentUserId) return Forbid();
        var count = await _memberService.CountUnreadMessagesAsync(conversationId, userId);

        return Ok(new { unreadCount = count });
    }

    [HttpGet]
    public async Task<IActionResult> GetMembers(string conversationId)
    {
        var members = await _memberService.GetMembersAsync(conversationId);
        if (!members.Any(m => m.UserId == CurrentUserId)) return Forbid();
        return Ok(members);
    }

    [HttpPatch("mute")]
    public async Task<IActionResult> UpdateMute(string conversationId, [FromBody] UpdateMuteRequest request)
    {
        var result = await _memberService.UpdateMuteAsync(conversationId, CurrentUserId, request.IsMuted);
        if (!result) return NotFound(new { message = "Không tìm thấy thành viên" });

        return Ok(new { message = "Cập nhật thông báo thành công" });
    }

    [HttpPatch("pin")]
    public async Task<IActionResult> UpdatePin(string conversationId, [FromBody] UpdatePinRequest request)
    {
        var result = await _memberService.UpdatePinAsync(conversationId, CurrentUserId, request.IsPinned);
        if (!result) return NotFound(new { message = "Không tìm thấy thành viên" });

        return Ok(new { message = "Cập nhật ghim thành công" });
    }

    [HttpPost]
    public async Task<IActionResult> AddMember(string conversationId, AddConversationMemberRequest request)
    {
        var member = await _memberService.GetMembersAsync(conversationId);
        if (!member.Any(m => m.UserId == CurrentUserId)) return Forbid();

        var result = await _memberService.AddMemberAsync(conversationId, request);
        if (!result) return BadRequest(new { message = "Không thể thêm thành viên" });

        return Ok(new { message = "Thêm thành viên thành công" });
    }

    [HttpDelete("{userId}")]
    public async Task<IActionResult> RemoveMember(string conversationId, string userId)
    {
        var conversation = await _conversationService.GetConversationByIdAsync(conversationId);
        if (conversation == null) return NotFound(new { message = "Không tìm thấy cuộc trò chuyện" });

        var isAdmin = await _memberService.IsMemberWithRoleAsync(conversationId, CurrentUserId, ConversationMemberRole.Admin);
        if (conversation.CreatedBy != CurrentUserId && !isAdmin) return Forbid();

        var result = await _memberService.RemoveMemberAsync(conversationId, userId);
        if (!result) return NotFound(new { message = "Không tìm thấy thành viên" });

        return Ok(new { message = "Xóa thành viên thành công" });
    }
}
