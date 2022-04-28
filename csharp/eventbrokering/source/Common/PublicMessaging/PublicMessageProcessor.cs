using Common.Commands;
using Common.GraphQL;
using Common.Models;
using Common.Processing;
using Common.Repositories;
using Common.Validation;
using Dolittle.SDK;

namespace Common.PublicMessaging;

public abstract class PublicMessageProcessor<TEntity, TCommand> : CommandProcessor<TEntity, TCommand>, IPublicMessageProcessor<TCommand>
    where TEntity : IEntity
    where TCommand : ICommand<IEntity>, new()
{
    public PublicMessageProcessor(IValidator<TEntity> entityValidator, IBusinessValidator<TCommand> businessValidator, IDolittleClient dolittleClient, INotificationPublisher notificationPublisher, ICollectionService collectionService)
        : base(entityValidator, businessValidator, dolittleClient, notificationPublisher, collectionService)
    {
    }

    public abstract string ForMessageType { get; }

    public abstract Task<bool> ProcessMessage(PublicMessage publicMessage);
}