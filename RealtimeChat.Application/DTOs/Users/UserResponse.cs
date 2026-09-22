using RealtimeChat.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace RealtimeChat.Application.DTOs.Users;

public class UserResponse
{
    public string Id { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string? AvatarUrl { get; set; }

    public string? Bio { get; set; }

    public string PhoneNumber { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public bool IsOnline { get; set; }

    public DateTime? LastSeenAt { get; set; }

    public static UserResponse FromEntity(User user) => new()
    {
        Id = user.Id,
        DisplayName = user.DisplayName,
        AvatarUrl = user.AvatarUrl,
        Bio = user.Bio,
        PhoneNumber = user.PhoneNumber,
        Email = user.Email,
        IsOnline = user.IsOnline,
        LastSeenAt = user.LastSeenAt,
    };
}
