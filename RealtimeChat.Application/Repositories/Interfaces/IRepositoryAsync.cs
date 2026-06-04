using MongoDB.Driver;
using RealtimeChat.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace RealtimeChat.Application.Repositories.Interfaces
{
    public interface IRepositoryAsync<TEntity>
        where TEntity : BaseEntity
    {
        Task<List<TEntity>> GetAllAsync();

        Task<TEntity?> GetByIdAsync(string id);

        Task<List<TEntity>> QueryConditionAsync(Expression<Func<TEntity, bool>> predicate);

        Task<List<TEntity>> FindAsync(FilterDefinition<TEntity> filter);

        Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate);

        Task AddAsync(TEntity entity);

        Task AddAsync(IEnumerable<TEntity> entities);

        Task UpdateAsync(string id, TEntity entity);

        Task DeleteAsync(string id);
    }
}
