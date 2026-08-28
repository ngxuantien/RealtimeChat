using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using RealtimeChat.API.Hubs;
using RealtimeChat.Application.DTOs.Messages;
using RealtimeChat.Application.Service.Interfaces;
using RealtimeChat.Application.Utils;
using RealtimeChat.Domain.Entities;

namespace RealtimeChat.API.Controllers;

[Authorize]
[ApiController]
[Route("api/messages")]
public class MessagesController : ControllerBase
{
    private readonly IMessageService _messageService;
    private readonly IConversationMemberService _memberService;
    private readonly IFileStorageService _fileStorageService;
    private readonly IHubContext<ChatHub> _hubContext;

    public MessagesController(IMessageService messageService, IConversationMemberService memberService, IFileStorageService fileStorageService, IHubContext<ChatHub> hubContext)
    {
        _messageService = messageService;
        _memberService = memberService;
        _fileStorageService = fileStorageService;
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

        var members = await _memberService.GetMembersAsync(result.ConversationId);
        var memberGroups = members.Select(m => $"user-{m.UserId}").ToList();

        await _hubContext.Clients.Groups(memberGroups).SendAsync("ConversationUpdated", new
        {
            conversationId = result.ConversationId,
            lastMessagePreview = MessagePreviewHelper.Build(result),
            lastMessageAt = result.CreatedAt,
        });

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
        if (result == null)
        {
            return BadRequest(new
            {
                message = "Cannot delete message",
            });
        }

        await _hubContext.Clients.Group(result.ConversationId).SendAsync("MessageDeleted", new
        {
            messageId = result.Id,
            conversationId = result.ConversationId,
        });

        return Ok(new { message = "Message deleted successfully" });
    }

    [HttpPost("attachments")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(11_000_000)]
    public async Task<IActionResult> UploadAttachment([FromForm] IFormFile file)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest(new { message = "Vui lòng chọn tệp" });
        }

        try
        {
            await using var stream = file.OpenReadStream();
            var attachment = await _fileStorageService.SaveMessageAttachmentAsync(stream, file.ContentType, file.FileName, file.Length);

            return Ok(attachment);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
