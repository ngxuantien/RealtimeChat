using RealtimeChat.Application.DTOs.Auths;
using RealtimeChat.Application.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace RealtimeChat.Application.Service;

public class AuthService : IAuthService
{
    public Task<bool> ChangePasswordAsync(string userId, ChangePasswordRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<AuthResponse?> LoginAsync(LoginRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<bool> LogoutAsync(string userId)
    {
        throw new NotImplementedException();
    }

    public Task<AuthResponse?> RefreshTokenAsync(RefreshTokenRequest request)
    {
        throw new NotImplementedException();
    }
}
