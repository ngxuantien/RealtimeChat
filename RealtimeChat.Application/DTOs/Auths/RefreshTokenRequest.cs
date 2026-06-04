using System;
using System.Collections.Generic;
using System.Text;

namespace RealtimeChat.Application.DTOs.Auths
{
    public class RefreshTokenRequest
    {
        public string RefreshToken { get; set; } = string.Empty;
    }
}
