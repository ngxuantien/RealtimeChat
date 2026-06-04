using System;
using System.Collections.Generic;
using System.Text;

namespace RealtimeChat.Application.DTOs.Auths
{
    public class AuthResponse
    {
        public string UserId { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string AccessToken { get; set; } = string.Empty;

        public string RefreshToken { get; set; } = string.Empty;
    }
}
