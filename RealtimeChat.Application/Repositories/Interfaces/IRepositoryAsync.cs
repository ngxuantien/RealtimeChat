using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace RealtimeChat.Application.Repositories.Interfaces
{
    public interface IRepositoryAsync<TEntity>
        where TEntity : class
    {
        IQueryable<TEntity> GetAll();

        IQueryable<TEntity> QueryCondition(Expression<Func<TEntity, bool>> predicate);

        Task<TEntity?> FindByIdAsync(string id);

        Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate);

        Task AddAsync(TEntity entity);

        Task AddAsync(IEnumerable<TEntity> entities);

        Task UpdateAsync(string id, TEntity entity);

        Task DeleteAsync(string id);
    }
}
