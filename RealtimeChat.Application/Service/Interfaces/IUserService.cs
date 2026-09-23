using RealtimeChat.Domain.Entities;
using RealtimeChat.Application.DTOs.Users;

namespace RealtimeChat.Application.Service.Interfaces;

public interface IUserService
{
    Task<User> CreateUserAsync(CreateUserRequest createUserRequest);

    Task<List<UserResponse>> GetAllUserAsync();

    Task<UserResponse?> GetUserByIdAsync(string id_user);

    Task<UserResponse?> GetUserByPhoneAsync(string phoneNumber);

    Task<List<UserResponse>> SearchUserAsync(string keyword);

    Task<bool> DeleteUserAsync(string id_user);

    Task<User?> UpdateUserAsync(string id_user, UpdateUserRequest request);
}
