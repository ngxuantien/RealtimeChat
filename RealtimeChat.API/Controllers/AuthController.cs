using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealtimeChat.Application.DTOs.Auths;
using RealtimeChat.Application.Service.Interfaces;

namespace RealtimeChat.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var authResponse = await _authService.LoginAsync(request);

        if (authResponse == null)
        {
            return Unauthorized();
        }

        return Ok(authResponse);
    }

    [HttpPost("logout/{userId}")]
    public async Task<IActionResult> Logout(string userId)
    {
        var result = await _authService.LogoutAsync(userId);

        if (!result)
        {
            return NotFound(new
            {
                message = "User not found"
            });
        }

        return Ok(new
        {
            message = "Logout successfully"
        });
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken(
        RefreshTokenRequest request)
    {
        var result = await _authService.RefreshTokenAsync(request);

        if (result == null)
        {
            return Unauthorized(new
            {
                message = "Invalid refresh token"
            });
        }

        return Ok(result);
    }

    [HttpPut("{userId}/change-password")]
    public async Task<IActionResult> ChangePassword(
        string userId,
        ChangePasswordRequest request)
    {
        var result = await _authService.ChangePasswordAsync(
            userId,
            request);

        if (!result)
        {
            return BadRequest(new
            {
                message = "Invalid user or password"
            });
        }

        return Ok(new
        {
            message = "Password changed successfully"
        });
    }
}
