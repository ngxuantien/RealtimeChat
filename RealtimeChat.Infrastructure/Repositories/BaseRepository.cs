using MongoDB.Driver;
using RealtimeChat.Infrastructure.Mongo;
using System;
using System.Collections.Generic;
using System.Text;

namespace RealtimeChat.Infrastructure.Repositories
{
    public abstract class BaseRepository<TEntity> where TEntity : class
    {
        protected BaseRepository(MongoDbContext context)
        {
            Context = context;
            Collection = context.GetCollection<TEntity>();
        }

        protected MongoDbContext Context { get; }

        protected IMongoCollection<TEntity> Collection { get; }
    }
}
