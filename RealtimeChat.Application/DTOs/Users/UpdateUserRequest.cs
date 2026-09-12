using System;
using System.Collections.Generic;
using System.Text;

namespace RealtimeChat.Application.DTOs.Users
{
    public class UpdateUserRequest
    {
        public string? DisplayName { get; set; }

        public string? Email { get; set; }

        public string? PhoneNumber { get; set; }

        public string? AvatarUrl { get; set; }

        public string? Bio { get; set; }
    }
}
