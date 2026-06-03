using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using RealtimeChat.Application.DTOs.Users;
using RealtimeChat.Application.Service.Interfaces;
using RealtimeChat.Domain.Entities;
using RealtimeChat.Domain.Enums;
using RealtimeChat.Infrastructure.Mongo;

namespace RealtimeChat.API.Controllers;

[ApiController]
[Route("api/test-mongo")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser(
        [FromBody] CreateUserRequest request)
    {
        var user = await _userService.CreateUserAsync(request);

        return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, user);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserById(string id)
    {
        var user = await _userService.GetUserByIdAsync(id);

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