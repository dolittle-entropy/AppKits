using Common.Exceptions;
using Common.GraphQL;
using Common.Helpers;
using Common.Messaging;
using Common.Models;
using Common.Repositories;
using Dolittle.SDK.Events;
using Serilog;

namespace Common.EventHandling
{
    /// <summary>
    /// Use as base class for EventHandlers that need to support aggregates,
    /// i.e. WorkOrders. This base class supports transactions, it can transmit public messages via an
    /// <see cref="IMessagePublisher"/>.
    /// This base class extends <see cref="NotificationEventHandler{TReadModel}"/>.
    /// </summary>
    public class TransactionalEventHandler<TReadModel> : NotificationEventHandler<TReadModel>
        where TReadModel : class, IEntity, new()
    {
        readonly ILogger _log;
        readonly IMessagePublisher _messagePublisher;

        public TransactionalEventHandler(
            ILogger log,
            INotificationPublisher notificationPublisher,
            IMessagePublisher messagePublisher,
            ICollectionService collectionService,
            string collectionName)
            : base(log, notificationPublisher, collectionService, collectionName)
        {
            _log = log.ForContext<TransactionalEventHandler<TReadModel>>();
            _messagePublisher = messagePublisher;
        }

        protected Task<bool> SendToBroker(PublicCommand publicCommand, CancellationToken token = default)
        {
            var retryPolicy = RetryPolicyBuilder.ForErpConnector(this, _log) ?? throw new RetryPolicyBuilderFailed();
            return retryPolicy.Execute(() => _messagePublisher.Publish(publicCommand));
        }

        protected PublicCommand CreatePublicDeleteCommand(EventContext eventContext, string eventHandlerName, object payload)
        {
            var tenantId = eventContext.CurrentExecutionContext.Tenant.Value;
            var sequenceNumber = (long)eventContext.SequenceNumber.Value;

            var command = new M3Command
            {
                CorrelationId = Guid.NewGuid().ToString(),
                EntityName = _modelName,
                TenantId = tenantId.ToString(),
                EventHandler = eventHandlerName,
                FromSequenceNumber = sequenceNumber,
                Payload = payload
            };
            return PublicCommandFactory.CreateDeleteCommand(command, this);
        }

        protected PublicCommand CreatePublicUpdateCommand(EventContext eventContext, string eventHandlerName, object payload)
        {
            var tenantId = eventContext.CurrentExecutionContext.Tenant.Value;
            var sequenceNumber = (long)eventContext.SequenceNumber.Value;

            var command = new M3Command
            {
                CorrelationId = Guid.NewGuid().ToString(),
                EntityName = _modelName,
                TenantId = tenantId.ToString(),
                EventHandler = eventHandlerName,
                FromSequenceNumber = sequenceNumber,
                Payload = payload
            };
            return PublicCommandFactory.CreateUpdateCommand(command, this);
        }
    }
}
