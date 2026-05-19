using RealtimeChat.Domain.Entities;
using RealtimeChat.Application.DTOs.Users;

namespace RealtimeChat.Application.Service.Interfaces;

public interface IUserService
{
    Task<User> CreateUserAsync(CreateUserRequest createUserRequest);

    Task<List<User>> GetAllUserAsync();

    Task<User?> GetUserByIdAsync(string id);

    Task<List<User>> SearchUserAsync(string keyword);   
}
