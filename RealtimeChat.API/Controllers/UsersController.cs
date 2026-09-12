using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealtimeChat.Application.DTOs.Users;
using RealtimeChat.Application.Service.Interfaces;

namespace RealtimeChat.API.Controllers;

[Route("api/users")]
public class UsersController : BaseApiController
{
    private readonly IUserService _userService;
    private readonly IFileStorageService _fileStorageService;

    public UsersController(IUserService userService, IFileStorageService fileStorageService)
    {
        _userService = userService;
        _fileStorageService = fileStorageService;
    }

    [AllowAnonymous]
    [HttpPost]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(6_000_000)]
    public async Task<IActionResult> CreateUser([FromForm] CreateUserRequest request, [FromForm] IFormFile avatar)
    {
        if (avatar is null || avatar.Length == 0)
        {
            return BadRequest(new { message = "Vui lòng chọn ảnh đại diện" });
        }

        try
        {
            await using var stream = avatar.OpenReadStream();
            request.AvatarUrl = await _fileStorageService.SaveAvatarAsync(stream, avatar.ContentType, avatar.Length);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }

        var user = await _userService.CreateUserAsync(request);

        if (user == null)
        {
            return BadRequest(new
            {
                message = "Email hoặc số điện thoại đã tồn tại"
            });
        }

        return Ok(user);
    }

    [HttpGet("{id_user}")]
    public async Task<IActionResult> GetUserById(string id_user)
    {
        var user = await _userService.GetUserByIdAsync(id_user);

        if (user == null)
        {
            return NotFound(new
            {
                message = "User not found"
            });
        }

        return Ok(user);
    }

    [HttpGet("phone/{phoneNumber}")]
    public async Task<IActionResult> GetUserByPhone(string phoneNumber)
    {
        var user = await _userService.GetUserByPhoneAsync(phoneNumber);

        if (user == null)
        {
            return NotFound(new
            {
                message = "Không tìm thấy người dùng với số điện thoại này"
            });
        }

        return Ok(user);
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _userService.GetAllUserAsync();

        return Ok(users);
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchUsers(
    [FromQuery] string keyword)
    {
        var users = await _userService.SearchUserAsync(keyword);

        return Ok(users);
    }

    [HttpDelete("{id_user}")]
    public async Task<IActionResult> DeleteUser(string id_user)
    {
        if (id_user != CurrentUserId) return Forbid();
        var result = await _userService.DeleteUserAsync(id_user);

        if (!result)
        {
            return NotFound(new
            {
                message = "User not found"
            });
        }

        return NoContent();
    }

    [HttpPut("{id_user}")]
    public async Task<IActionResult> UpdateUser(string id_user, [FromBody] UpdateUserRequest request)
    {
        if (id_user != CurrentUserId) return Forbid();

        try
        {
            var user = await _userService.UpdateUserAsync(id_user, request);

            if (user == null)
                return NotFound(new { message = "User not found" });

            return Ok(user);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id_user}/avatar")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(6_000_000)]
    public async Task<IActionResult> UpdateAvatar(string id_user, [FromForm] IFormFile avatar)
    {
        if (id_user != CurrentUserId) return Forbid();

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

        var user = await _userService.UpdateUserAsync(id_user, new UpdateUserRequest { AvatarUrl = avatarUrl });

        if (user == null)
            return NotFound(new { message = "User not found" });

        return Ok(user);
    }
}