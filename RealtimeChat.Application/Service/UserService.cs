using BCrypt.Net;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
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
        var result = _unitOfWork.GetRepositoryAsync<User>();

        return result.GetAllAsync();
    }

    public async Task<User?> GetUserByIdAsync(string id_user)
    {
        var result = _unitOfWork.GetRepositoryAsync<User>();

        return await result.GetByIdAsync(id_user);
    }

    public async Task<List<User>> SearchUserAsync(string keyword)
    {
        var filter = Builders<User>.Filter.Or(
            Builders<User>.Filter.Regex(
                x => x.DisplayName,
                new BsonRegularExpression(keyword, "i")),
            Builders<User>.Filter.Regex(
                x => x.Email,
                new BsonRegularExpression(keyword, "i"))
        );

        return await _unitOfWork.GetRepositoryAsync<User>().FindAsync(filter);
    }

    public async Task<bool> DeleteUserAsync(string id_user)
    {
        var repository = _unitOfWork.GetRepositoryAsync<User>();

        var user = repository.GetByIdAsync(id_user);

        if (user == null)
        {
            return false;
        }

        await repository.DeleteAsync(id_user);
        return true;
    }

    public async Task<User?> UpdateUserAsync(string id_user, UpdateUserRequest request)
    {
        var repository = _unitOfWork.GetRepositoryAsync<User>();

        var user = await repository.GetByIdAsync(id_user);

        if(user == null)
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(request.DisplayName))
            user.DisplayName = request.DisplayName;

        if (!string.IsNullOrWhiteSpace(request.AvatarUrl))
            user.AvatarUrl = request.AvatarUrl;

        if (!string.IsNullOrWhiteSpace(request.Bio))
            user.Bio = request.Bio;

        user.UpdatedAt = DateTime.UtcNow;

        await repository.UpdateAsync(id_user, user);

        return user;
    }
}
