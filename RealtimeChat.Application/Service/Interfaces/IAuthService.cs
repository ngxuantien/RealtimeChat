using RealtimeChat.Application.DTOs.Auths;
using System;
using System.Collections.Generic;
using System.Text;

namespace RealtimeChat.Application.Service.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponse?> LoginAsync(LoginRequest request);

        Task<AuthResponse?> RefreshTokenAsync(RefreshTokenRequest request);

        Task<bool> LogoutAsync(string userId);

        Task<bool> ChangePasswordAsync(string userId, ChangePasswordRequest request);
    }
}
