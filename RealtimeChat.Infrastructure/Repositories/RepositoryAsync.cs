using MongoDB.Driver;
using RealtimeChat.Application.Repositories.Interfaces;
using RealtimeChat.Infrastructure.Mongo;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace RealtimeChat.Infrastructure.Repositories
{
    public class RepositoryAsync<TEntity>
        : BaseRepository<TEntity>,
        IRepositoryAsync<TEntity>
        where TEntity : class
    {
        public RepositoryAsync(MongoDbContext context) : base(context)
        {
        }

        public IQueryable<TEntity> GetAll()
        {
            return Collection.AsQueryable();
        }

        public IQueryable<TEntity> QueryCondition(Expression<Func<TEntity, bool>> predicate)
        {
            return Collection.AsQueryable().Where(predicate);
        }

        public Task AddAsync(TEntity entity)
        {
            return Collection.InsertOneAsync(entity);
        }

        public Task AddAsync(IEnumerable<TEntity> entities)
        {
            return Collection.InsertManyAsync(entities);
        }

        public async Task DeleteAsync(string id)
        {
            var filter = Builders<TEntity>.Filter.Eq("_id", id);

            await Collection.DeleteOneAsync(filter);
        }

        public async Task<TEntity?> FindByIdAsync(string id)
        {
            var filter = Builders<TEntity>.Filter.Eq("_id", id);

            return await Collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await Collection.Find(predicate).FirstOrDefaultAsync();
        }

        public async Task UpdateAsync(string id, TEntity entity)
        {
            var filter = Builders<TEntity>.Filter.Eq("_id", id);

            await Collection.ReplaceOneAsync(filter, entity);
        }
    }
}
