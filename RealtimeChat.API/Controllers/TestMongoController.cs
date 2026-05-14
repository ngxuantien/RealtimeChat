using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using RealtimeChat.Domain.Entities;
using RealtimeChat.Domain.Enums;
using RealtimeChat.Infrastructure.Mongo;

namespace RealtimeChat.API.Controllers;

[ApiController]
[Route("api/test-mongo")]
public class TestMongoController : ControllerBase
{
    private readonly MongoDbContext _mongoDbContext;

    public TestMongoController(MongoDbContext mongoDbContext)
    {
        _mongoDbContext = mongoDbContext;
    }

    [HttpPost("create-user")]
    public async Task<IActionResult> CreateUser()
    {
        var user = new User
        {
            DisplayName = "Demo User",
            Email = $"demo-{Guid.NewGuid():N}@gmail.com",
            PasswordHash = "demo_hash",
            AvatarUrl = null,
            Bio = "Test MongoDB connection",
            Status = UserStatus.Active,
            IsOnline = false,
            LastSeenAt = null,
            CreatedAt = DateTime.UtcNow
        };

        await _mongoDbContext.Users.InsertOneAsync(user);

        return Ok(user);
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _mongoDbContext.Users
            .Find(x => x.DeletedAt == null)
            .ToListAsync();

        return Ok(users);
    }
}