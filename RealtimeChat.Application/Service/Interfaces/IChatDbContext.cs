using MongoDB.Driver;
using RealtimeChat.Domain.Entities;

namespace RealtimeChat.Application.Service.Interfaces;

public interface IChatDbContext
{
    IMongoCollection<User> Users { get; }
}
