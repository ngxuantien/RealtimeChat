using MongoDB.Driver;
using RealtimeChat.Application.Repositories.Interfaces;
using RealtimeChat.Domain.Entities;
using RealtimeChat.Infrastructure.Mongo;
using System;
using System.Collections.Generic;
using System.Text;

namespace RealtimeChat.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly MongoDbContext _dbContext;
        private readonly Dictionary<Type, object> _repositories = new();
        public UnitOfWork(MongoDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IRepositoryAsync<TEntity> GetRepositoryAsync<TEntity>()
            where TEntity : BaseEntity
        {
            var type = typeof(TEntity);

            if (!_repositories.ContainsKey(type))
            {
                _repositories[type] = new RepositoryAsync<TEntity>(_dbContext);
            }

            return (IRepositoryAsync<TEntity>)_repositories[type];
        }
    }
}
