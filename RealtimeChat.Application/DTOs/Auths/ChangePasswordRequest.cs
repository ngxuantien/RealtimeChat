using System;
using System.Collections.Generic;
using System.Text;

namespace RealtimeChat.Application.DTOs.Auths
{
    public class ChangePasswordRequest
    {
        public string CurrentPassword { get; set; } = string.Empty;

        public string NewPassword { get; set; } = string.Empty;
    }
}
