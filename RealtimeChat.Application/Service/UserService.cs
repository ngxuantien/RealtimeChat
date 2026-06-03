using BCrypt.Net;
using MongoDB.Driver;
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
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(createUserRequest.Password)
        };

        try
        {
            await userRepo.AddAsync(user);
            return new User
            {
                Id = user.Id,
                DisplayName = user.DisplayName,
                Email = user.Email,
                AvatarUrl = user.AvatarUrl,
                Bio = user.Bio,
                IsOnline = user.IsOnline
            };
        }
        catch (MongoWriteException ex)
            when (ex.WriteError?.Category ==
                  ServerErrorCategory.DuplicateKey)
        {
            throw new Exception("Email already exists");
        }
    }

    public Task<List<User>> GetAllUserAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<User?> GetUserByIdAsync(string id)
    {
        var result = _unitOfWork.GetRepositoryAsync<User>();

        return await result.FindByIdAsync(id);
    }

    public Task<List<User>> SearchUserAsync(string keyword)
    {
        throw new NotImplementedException();
    }
}
