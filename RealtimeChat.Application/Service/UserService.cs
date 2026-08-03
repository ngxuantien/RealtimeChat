using BCrypt.Net;
using Microsoft.Extensions.Logging;
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
    private readonly ILogger<UserService> _logger;

    public UserService(IUnitOfWork unitOfWork, ILogger<UserService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<User> CreateUserAsync(CreateUserRequest createUserRequest)
    {
        var userRepo = _unitOfWork.GetRepositoryAsync<User>();

        var existedUser = await userRepo.FirstOrDefaultAsync(x => x.Email == createUserRequest.Email || x.PhoneNumber == createUserRequest.PhoneNumber);

        if (existedUser != null)
        {
            return null;
        }

        var user = new User
        {
            DisplayName = createUserRequest.DisplayName,
            Email = createUserRequest.Email,
            PhoneNumber = createUserRequest.PhoneNumber,
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
                PhoneNumber = user.PhoneNumber,
                AvatarUrl = user.AvatarUrl,
                Bio = user.Bio,
                IsOnline = user.IsOnline
            };
        }
        catch (MongoWriteException ex)
            when (ex.WriteError?.Category ==
                  ServerErrorCategory.DuplicateKey)
        {
            throw new Exception("Email or PhoneNumber already exists");
        }
    }

    public async Task<User?> GetUserByPhoneAsync(string phoneNumber)
    {
        var userRepo = _unitOfWork.GetRepositoryAsync<User>();

        return await userRepo.FirstOrDefaultAsync(x => x.PhoneNumber == phoneNumber);
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
