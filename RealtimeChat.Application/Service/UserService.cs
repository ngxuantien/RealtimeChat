using RealtimeChat.Application.DTOs.Users;
using RealtimeChat.Application.Service.Interfaces;
using RealtimeChat.Domain.Entities;


namespace RealtimeChat.Application.Service;

public class UserService : IUserService
{
    private readonly IChatDbContext _context;

    public UserService(IChatDbContext context)
    {
        _context = context;
    }

    public Task<User> CreateUserAsync(CreateUserRequest createUserRequest)
    {
        throw new NotImplementedException();
    }

    public Task<List<User>> GetAllUserAsync()
    {
        throw new NotImplementedException();
    }

    public Task<User?> GetUserByIdAsync(string id)
    {
        throw new NotImplementedException();
    }

    public Task<List<User>> SearchUserAsync(string keyword)
    {
        throw new NotImplementedException();
    }
}
