using MongoDB.Bson;
using MongoDB.Driver;
using RealtimeChat.Application.Repositories.Interfaces;
using RealtimeChat.Domain.Entities;
using RealtimeChat.Infrastructure.Mongo;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace RealtimeChat.Infrastructure.Repositories;

public class RepositoryAsync<TEntity>
    : BaseRepository<TEntity>,
    IRepositoryAsync<TEntity>
    where TEntity : BaseEntity
{
    public RepositoryAsync(MongoDbContext context) : base(context)
    {
    }

    public async Task<List<TEntity>> GetAllAsync()
    {
        return await Collection.Find(_ => true).ToListAsync();
    }

    public async Task<List<TEntity>> QueryConditionAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return await Collection.Find(predicate).ToListAsync();
    }

    public async Task<List<TEntity>> FindAsync(FilterDefinition<TEntity> filter)
    {
        return await Collection.Find(filter).ToListAsync();
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
        await Collection.DeleteOneAsync(x => x.Id == id);
    }

    public async Task<TEntity?> GetByIdAsync(string id)
    {
        return await Collection.Find(x => x.Id == id).FirstOrDefaultAsync();
    }

    public async Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return await Collection.Find(predicate).FirstOrDefaultAsync();
    }

    public async Task UpdateAsync(string id, TEntity entity)
    {
        await Collection.ReplaceOneAsync(x => x.Id == id, entity);
    }
}
