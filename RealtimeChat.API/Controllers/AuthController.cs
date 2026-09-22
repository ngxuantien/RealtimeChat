using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using RealtimeChat.Application.DTOs.Auths;
using RealtimeChat.Application.Service.Interfaces;

namespace RealtimeChat.API.Controllers;

[Route("api/auth")]
public class AuthController : BaseApiController
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [AllowAnonymous]
    [EnableRateLimiting("login")]
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
        if (userId != CurrentUserId) return Forbid();

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

    [AllowAnonymous]
    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken(RefreshTokenRequest request)
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

    [AllowAnonymous]
    [EnableRateLimiting("forgot-password")]
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest request)
    {
        await _authService.ForgotPasswordAsync(request);
        return Ok(new { message = "Nếu email tồn tại, chúng tôi đã gửi link đặt lại mật khẩu." });
    }

    [AllowAnonymous]
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequest request)
    {
        var result = await _authService.ResetPasswordAsync(request);

        if (!result)
            return BadRequest(new { message = "Liên kết đặt lại mật khẩu không hợp lệ hoặc đã hết hạn." });

        return Ok(new { message = "Đặt lại mật khẩu thành công." });
    }
}
