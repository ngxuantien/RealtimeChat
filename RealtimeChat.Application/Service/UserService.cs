using RealtimeChat.Application.DTOs.Users;
using RealtimeChat.Application.Repositories.Interfaces;
using RealtimeChat.Application.Service.Interfaces;
using RealtimeChat.Domain.Entities;


namespace RealtimeChat.Application.Service;

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;

    public UserService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<User> CreateUserAsync(CreateUserRequest createUserRequest)
    {
        var userRepo = _unitOfWork.GetRepositoryAsync<User>();

        var existedUser = await userRepo.FirstOrDefaultAsync(x => x.Email == createUserRequest.Email);

        if (existedUser != null)
        {
            throw new Exception("Email already exists");
        }

        var user = new User
        {
            DisplayName = createUserRequest.DisplayName,
            Email = createUserRequest.Email,
            AvatarUrl = createUserRequest.AvatarUrl,
            Bio = createUserRequest.Bio,
            CreatedAt = DateTime.UtcNow
        };

        await userRepo.AddAsync(user);

        return user;
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
