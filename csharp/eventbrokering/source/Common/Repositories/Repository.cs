using Common.Models;
using MongoDB.Driver;
using Serilog;
using System.Linq.Expressions;

namespace Common.Repositories
{
    public class Repository<TEntity> : IRepository<TEntity>
        where TEntity : class, IEntity, new()
    {
        readonly IMongoCollection<TEntity> _collection;
        readonly ILogger _log;

        public Repository(IMongoCollection<TEntity> collection)
        {
            _collection = collection ?? throw new ArgumentNullException(nameof(collection));
            _log = Log.ForContext<Repository<TEntity>>();
        }

        public IMongoCollection<TEntity> GetCollection
            => _collection;

        public async Task<bool> Delete(Expression<Func<TEntity, bool>> filter, CancellationToken token = default)
        {
            if (await _collection.DeleteManyAsync(filter, token) is DeleteResult result)
                if (result.IsAcknowledged && result.DeletedCount > 0)
                    return true;

            return false;
        }

        public async Task<IEnumerable<TEntity>> GetAll(Expression<Func<TEntity, bool>> filter, CancellationToken token = default)
            => await _collection.Find(filter).ToListAsync();

        public async Task<TEntity?> GetOne(Expression<Func<TEntity, bool>> filter, CancellationToken token = default)
            => await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken: token) is TEntity entity
            ? entity
            : default;

        public async Task<TEntity?> Upsert(Expression<Func<TEntity, bool>> filter, TEntity entity, CancellationToken token = default)
        {
            var replaceOptions = new ReplaceOptions
            {
                IsUpsert = true
            };
            if (await _collection.ReplaceOneAsync(filter, entity, replaceOptions, token) is ReplaceOneResult result)
            {
                if (result.IsAcknowledged)
                    return entity;
            }
            return default;
        }

        public async Task<TEntity?> UpsertOne(Expression<Func<TEntity, bool>> filter, TEntity entity, CancellationToken token = default)
        {
            var options = new FindOneAndReplaceOptions<TEntity>
            {
                IsUpsert = true
            };

            await _collection.FindOneAndReplaceAsync(filter, entity, options, token);
            return entity;
        }

    }
}
