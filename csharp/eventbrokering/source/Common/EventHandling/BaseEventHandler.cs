using Common.Exceptions;
using Common.Helpers;
using Common.Models;
using Common.Repositories;
using Dolittle.SDK.Resources;
using MongoDB.Driver;
using Serilog;
using System.Linq.Expressions;

namespace Common.EventHandling
{
    /// <summary>
    /// Base class for all eventhandlers. Cannot be inherited from directly.
    /// Instead, use one of:  <see cref="NotificationEventHandler{TReadModel}"/> or <see cref="TransactionalEventHandler{TReadModel}"/>
    /// </summary>
    public class BaseEventHandler<TReadModel> where TReadModel : class, IEntity, new()
    {
        readonly ILogger _log;
        readonly IRepository<TReadModel> _repo;
        protected readonly string _modelName;
        protected readonly string _collectionName;

        public BaseEventHandler(ILogger log, ICollectionService collectionService, string collectionName)
        {
            _log = log.ForContext<BaseEventHandler<TReadModel>>();
            _repo = collectionService.GetRepository<TReadModel>(collectionName);
            _collectionName = collectionName;
            _modelName = typeof(TReadModel).Name;
        }
        public IRepository<TReadModel> Repository => _repo;

        public BaseEventHandler(ILogger log, IResources resources, ICollectionService collectionService, string collectionName)
        {
            _log = log.ForContext<BaseEventHandler<TReadModel>>();
            _repo = collectionService.GetRepository<TReadModel>(collectionName);
            _collectionName = collectionName;
            _modelName = typeof(TReadModel).Name;
        }

        public virtual Task<TReadModel?> CreateReadModel(Expression<Func<TReadModel, bool>> filter, TReadModel model, CancellationToken token = default)
        {
            var retryPolicy = RetryPolicyBuilder.ForDataWriter(this, _log) ?? throw new RetryPolicyBuilderFailed();
            return retryPolicy.Execute(() => Repository.Upsert(filter, model, token));
        }

        public virtual Task<TReadModel?> CreateReadModelNonBulk(Expression<Func<TReadModel, bool>> filter,
            TReadModel model, CancellationToken token = default)
        {
            var retryPolicy = RetryPolicyBuilder.ForDataWriter(this, _log) ?? throw new RetryPolicyBuilderFailed();
            return retryPolicy.Execute(() => Repository.UpsertOne(filter, model, token));
        }

        public virtual Task<TReadModel?> UpdateReadModel(Expression<Func<TReadModel, bool>> filter, TReadModel model, CancellationToken token = default)
        {
            var retryPolicy = RetryPolicyBuilder.ForDataWriter(this, _log) ?? throw new RetryPolicyBuilderFailed();
            return retryPolicy.Execute(() => Repository.Upsert(filter, model, token));
        }

        public virtual async Task<bool> UpdateSingleField<TField>(Expression<Func<TReadModel, bool>> findFilter, Expression<Func<TReadModel, TField>> fieldToUpdate, TField newValue)
        {
            var updateExpression = Builders<TReadModel>.Update.Set(fieldToUpdate, newValue);
            var options = new FindOneAndUpdateOptions<TReadModel>
            {
                IsUpsert = false
            };
            return await Repository.GetCollection.FindOneAndUpdateAsync(findFilter, updateExpression, options) is TReadModel
                ? true
                : false;
        }

        protected Task<bool> DeleteReadModel(Expression<Func<TReadModel, bool>> filter, CancellationToken token = default)
        {
            var retryPolicy = RetryPolicyBuilder.ForDataWriter(this, _log) ?? throw new RetryPolicyBuilderFailed();
            return retryPolicy.Execute(() => Repository.Delete(filter, token));
        }
    }
}
