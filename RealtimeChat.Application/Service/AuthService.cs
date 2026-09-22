using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using RealtimeChat.Application.DTOs.Auths;
using RealtimeChat.Application.Repositories.Interfaces;
using RealtimeChat.Application.Service.Interfaces;
using RealtimeChat.Domain.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace RealtimeChat.Application.Service;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;
    private readonly IEmailSender _emailSender;

    public AuthService(IUnitOfWork unitOfWork, IConfiguration configuration, IEmailSender emailSender)
    {
        _unitOfWork = unitOfWork;
        _configuration = configuration;
        _emailSender = emailSender;
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request)
    {
        var repository = _unitOfWork.GetRepositoryAsync<User>();

        var user = await repository.FirstOrDefaultAsync(x => x.PhoneNumber == request.PhoneNumber);

        if (user == null)
        {
            return null;
        }

        var isValidPassword = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);

        if (!isValidPassword)
        {
            return null;
        }

        user.IsOnline = true;
        user.LastSeenAt = DateTime.UtcNow;
        user.RefreshToken = GenerateRefreshToken();
        user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);
        user.UpdatedAt = DateTime.UtcNow;

        await repository.UpdateAsync(user.Id, user);

        return new AuthResponse
        {
            UserId = user.Id,
            DisplayName = user.DisplayName,
            Email = user.Email,
            AccessToken = GenerateAccessToken(user),
            RefreshToken = user.RefreshToken
        };
    }

    public async Task<bool> LogoutAsync(string userId)
    {
        var repository = _unitOfWork.GetRepositoryAsync<User>();

        var user = await repository.GetByIdAsync(userId);

        if (user == null)
            return false;

        user.RefreshToken = null;
        user.RefreshTokenExpiresAt = null;
        user.IsOnline = false;
        user.LastSeenAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;

        await repository.UpdateAsync(user.Id, user);

        return true;
    }

    public async Task<AuthResponse?> RefreshTokenAsync(RefreshTokenRequest request)
    {
        var repository = _unitOfWork.GetRepositoryAsync<User>();

        var user = await repository.FirstOrDefaultAsync(x => x.RefreshToken == request.RefreshToken);

        if(user == null)
        {
            return null;
        }

        if(user.RefreshTokenExpiresAt < DateTime.UtcNow)
        {
            return null;
        }

        user.RefreshToken = GenerateRefreshToken();
        user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);
        user.UpdatedAt = DateTime.UtcNow;

        await repository.UpdateAsync(user.Id, user);

        return new AuthResponse
        {
            UserId = user.Id,
            DisplayName = user.DisplayName,
            Email = user.Email,
            AccessToken = GenerateAccessToken(user),
            RefreshToken = user.RefreshToken
        };
    }

    public async Task<bool> ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        var repository = _unitOfWork.GetRepositoryAsync<User>();

        var user = await repository.FirstOrDefaultAsync(x => x.Email == request.Email);

        // Luôn trả về true dù không tìm thấy, tránh lộ email nào đã đăng ký
        if (user == null) return true;

        user.PasswordResetToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        user.PasswordResetTokenExpiresAt = DateTime.UtcNow.AddMinutes(30);
        user.UpdatedAt = DateTime.UtcNow;

        await repository.UpdateAsync(user.Id, user);

        var frontendBaseUrl = _configuration["App:FrontendBaseUrl"];
        var resetLink = $"{frontendBaseUrl}/auth/reset-password?token={user.PasswordResetToken}";

        var html = $"<p>Xin chào {user.DisplayName},</p>" +
                   $"<p>Nhấn vào liên kết bên dưới để đặt lại mật khẩu. Liên kết có hiệu lực trong 30 phút:</p>" +
                   $"<p><a href=\"{resetLink}\">{resetLink}</a></p>" +
                   $"<p>Nếu bạn không yêu cầu đổi mật khẩu, hãy bỏ qua email này.</p>";

        await _emailSender.SendAsync(user.Email, "Đặt lại mật khẩu ChatFlow", html);

        return true;
    }

    public async Task<bool> ResetPasswordAsync(ResetPasswordRequest request)
    {
        var repository = _unitOfWork.GetRepositoryAsync<User>();

        var user = await repository.FirstOrDefaultAsync(x => x.PasswordResetToken == request.Token);

        if (user == null) return false;
        if (user.PasswordResetTokenExpiresAt == null || user.PasswordResetTokenExpiresAt < DateTime.UtcNow)
            return false;

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.PasswordResetToken = null;
        user.PasswordResetTokenExpiresAt = null;
        user.RefreshToken = null;
        user.RefreshTokenExpiresAt = null;
        user.UpdatedAt = DateTime.UtcNow;

        await repository.UpdateAsync(user.Id, user);

        return true;
    }

    private string GenerateAccessToken(User user)
    {
        var jwtKey = _configuration["Jwt:Key"];
        var issuer = _configuration["Jwt:Issuer"];
        var audience = _configuration["Jwt:Audience"];

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.DisplayName)
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtKey));

        var credentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(30),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler()
            .WriteToken(token);
    }

    private static string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];

        using var rng = RandomNumberGenerator.Create();

        rng.GetBytes(randomBytes);

        return Convert.ToBase64String(randomBytes);
    }
}