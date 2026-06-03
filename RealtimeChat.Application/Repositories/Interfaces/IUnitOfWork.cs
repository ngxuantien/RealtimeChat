using System;
using System.Collections.Generic;
using System.Text;

namespace RealtimeChat.Application.Repositories.Interfaces
{
    public interface IUnitOfWork
    {
        IRepositoryAsync<TEntity> GetRepositoryAsync<TEntity>() where TEntity : class;
    }
}
