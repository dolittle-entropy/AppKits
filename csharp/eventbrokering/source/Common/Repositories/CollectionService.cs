using Common.Commands;
using Common.Extensions;
using Common.Models;
using Dolittle.SDK.Resources;
using MongoDB.Driver;

namespace Common.Repositories
{
    public class CollectionService : ICollectionService
    {
        object _lock = new object();

        readonly Dictionary<string, object> _collections = new();
        readonly IResources? _resources;

        public CollectionService(IResources resources)
        {
            _resources = resources ?? throw new ArgumentNullException(nameof(resources));
        }

        public IRepository<TModel> GetRepository<TModel>(string collectionName) where TModel : class, IEntity, new()
        {
            if (string.IsNullOrEmpty(collectionName))
                throw new ArgumentNullException(nameof(collectionName));

            lock (_lock)
            {
                if (!_collections.ContainsKey(collectionName))
                {
                    var db = _resources!.MongoDB.GetDatabase();
                    var collection = db.GetCollection<TModel>(collectionName);
                    _collections.Add(collectionName, collection);
                }
            }

            IMongoCollection<TModel>? resultingCollection = null;
            if (_collections[collectionName] is not IMongoCollection<TModel> match)
            {
                ValidationExtensions.ThrowRepositoryNotFound(collectionName);
            }
            else
            {
                resultingCollection = match;
            }
            return new Repository<TModel>(resultingCollection!);
        }

        public IMongoCollection<TModel>? GetMongoCollection<TModel>(string collectionName)
        {
            lock (_lock)
            {
                if (!_collections.ContainsKey(collectionName))
                {
                    var db = _resources!.MongoDB.GetDatabase();
                    var collection = db.GetCollection<TModel>(collectionName);
                    _collections.Add(collectionName, collection);
                }
            }
            return _collections[collectionName] as IMongoCollection<TModel>;
        }

        public ICachedRepository<TModel, TCommand> GetCachedRepository<TCommand, TModel>(string collectionName)
            where TCommand : class, ICorrelatedCommand<TModel>, new()
            where TModel : class, IEntity, new()
        {
            var cachedCollectionName = $"Cached{collectionName}";
            if (string.IsNullOrEmpty(cachedCollectionName))
                throw new ArgumentNullException(nameof(cachedCollectionName));

            lock (_lock)
            {
                if (!_collections.ContainsKey(cachedCollectionName))
                {
                    var db = _resources!.MongoDB.GetDatabase();
                    var collection = db.GetCollection<TCommand>(cachedCollectionName);
                    _collections.Add(cachedCollectionName, collection);
                }
            }

            IMongoCollection<TCommand>? resultingCollection = null;
            if (_collections[cachedCollectionName] is not IMongoCollection<TCommand> match)
            {
                ValidationExtensions.ThrowRepositoryNotFound(cachedCollectionName);
            }
            else
            {
                resultingCollection = match;
            }
            return new CachedRepository<TModel, TCommand>(resultingCollection!);
        }
    }
}
