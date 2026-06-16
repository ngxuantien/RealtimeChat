using Microsoft.AspNetCore.Mvc;
using RealtimeChat.Application.DTOs.Users;
using RealtimeChat.Application.Service.Interfaces;
using RealtimeChat.Domain.Entities;
using RealtimeChat.Domain.Enums;
using RealtimeChat.Infrastructure.Mongo;

namespace RealtimeChat.API.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        var user = await _userService.CreateUserAsync(request);

        if (user == null)
        {
            return BadRequest(new
            {
                message = "Email already exists"
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
        var user = await _userService.UpdateUserAsync(id_user, request);

        if (user == null)
        {
            return NotFound(new
            {
                message = "User not found"
            });
        }

        return Ok(user);
    }
}