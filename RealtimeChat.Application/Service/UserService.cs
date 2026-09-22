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
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(createUserRequest.Password),
            AvatarUrl = createUserRequest.AvatarUrl
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

    public async Task<UserResponse?> GetUserByPhoneAsync(string phoneNumber)
    {
        var userRepo = _unitOfWork.GetRepositoryAsync<User>();
        var user = await userRepo.FirstOrDefaultAsync(x => x.PhoneNumber == phoneNumber && x.DeletedAt == null);
        return user == null ? null : UserResponse.FromEntity(user);
    }

    public async Task<List<UserResponse>> GetAllUserAsync()
    {
        var userRepo = _unitOfWork.GetRepositoryAsync<User>();
        var users = await userRepo.GetAllAsync();
        return users.Where(u => u.DeletedAt == null).Select(UserResponse.FromEntity).ToList();
    }

    public async Task<UserResponse?> GetUserByIdAsync(string id_user)
    {
        var result = _unitOfWork.GetRepositoryAsync<User>();
        var user = await result.GetByIdAsync(id_user);
        return user == null || user.DeletedAt != null ? null : UserResponse.FromEntity(user);
    }

    public async Task<List<UserResponse>> SearchUserAsync(string keyword)
    {
        var filter = Builders<User>.Filter.Or(
            Builders<User>.Filter.Regex(x => x.DisplayName, new BsonRegularExpression(keyword, "i")),
            Builders<User>.Filter.Regex(x => x.Email, new BsonRegularExpression(keyword, "i"))
        );

        var users = await _unitOfWork.GetRepositoryAsync<User>().FindAsync(filter);
        return users.Where(u => u.DeletedAt == null).Select(UserResponse.FromEntity).ToList();
    }

    public async Task<bool> DeleteUserAsync(string id_user)
    {
        var repository = _unitOfWork.GetRepositoryAsync<User>();

        var user = await repository.GetByIdAsync(id_user);

        if (user == null || user.DeletedAt != null)
        {
            return false;
        }

        user.DeletedAt = DateTime.UtcNow;
        user.RefreshToken = null;
        user.RefreshTokenExpiresAt = null;
        user.UpdatedAt = DateTime.UtcNow;

        await repository.UpdateAsync(id_user, user);
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

        if (!string.IsNullOrWhiteSpace(request.Email))
            user.Email = request.Email;

        if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
            user.PhoneNumber = request.PhoneNumber;

        if (!string.IsNullOrWhiteSpace(request.AvatarUrl))
            user.AvatarUrl = request.AvatarUrl;

        if (!string.IsNullOrWhiteSpace(request.Bio))
            user.Bio = request.Bio;

        user.UpdatedAt = DateTime.UtcNow;

        try
        {
            await repository.UpdateAsync(id_user, user);
        }
        catch (MongoWriteException ex)
            when (ex.WriteError?.Category == ServerErrorCategory.DuplicateKey)
        {
            throw new Exception("Email hoặc số điện thoại đã được sử dụng");
        }

        return user;
    }
}
