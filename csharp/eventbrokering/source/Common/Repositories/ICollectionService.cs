using Common.Commands;
using Common.Models;
using MongoDB.Driver;

namespace Common.Repositories
{
    public interface ICollectionService
    {
        IMongoCollection<TModel>? GetMongoCollection<TModel>(string collectionName);

        IRepository<TModel> GetRepository<TModel>(string collectionName) where TModel : class, IEntity, new();

        ICachedRepository<TEntity, TCommand> GetCachedRepository<TCommand, TEntity>(string collectionName)
            where TCommand : class, ICorrelatedCommand<TEntity>, new()
            where TEntity : class, IEntity, new()
            ;
    }
}
