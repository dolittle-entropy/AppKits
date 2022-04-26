using Common.Models;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace Common.Repositories
{
    public interface IRepository<TEntity>
        where TEntity : class, IEntity, new()
    {
        Task<IEnumerable<TEntity>> GetAll(Expression<Func<TEntity, bool>> filter, CancellationToken token = default);

        Task<TEntity?> GetOne(Expression<Func<TEntity, bool>> filter, CancellationToken token = default);

        Task<TEntity?> Upsert(Expression<Func<TEntity, bool>> filter, TEntity entity, CancellationToken token = default);

        Task<TEntity?> UpsertOne(Expression<Func<TEntity, bool>> filter, TEntity entity,
            CancellationToken token = default);

        Task<bool> Delete(Expression<Func<TEntity, bool>> filter, CancellationToken token = default);

        IMongoCollection<TEntity> GetCollection { get; }
    }
}
