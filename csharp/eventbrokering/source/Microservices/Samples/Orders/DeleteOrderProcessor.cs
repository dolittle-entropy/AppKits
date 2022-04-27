using Common.Commands;
using Common.Extensions;
using Common.GraphQL;
using Common.Repositories;
using Common.Validation;
using Dolittle.SDK;
using ILogger = Serilog.ILogger;

namespace Sample.Orders.Orders
{
    public class DeleteOrderProcessor : CommandProcessor<Order, DeleteOrder>
    {
        readonly ILogger _log;

        public DeleteOrderProcessor(
            ILogger log,
            IValidator<Order> entityValidator, 
            IBusinessValidator<DeleteOrder> businessValidator, 
            IDolittleClient dolittleClient, 
            INotificationPublisher notificationPublisher, 
            ICollectionService collectionService) 
            : base(entityValidator, businessValidator, dolittleClient, notificationPublisher, collectionService)
        {
            _log = log.ForContext<DeleteOrderProcessor>();
        }

        public override async Task<bool> Process(DeleteOrder command, CancellationToken cancellationToken = default)
        {
            if (!await CommandIsValid(command, cancellationToken))
                return false;

            _log.Enter(this, $"Creating {nameof(Order)} with Id: {command.Payload!.Id}");

            return await DoOnAggregate<OrdersAggregate>(
                eventSourceId: command.Payload.Id.ToString(),
                command: command,
                aggregateAction: act => act.Process(command))
                .ConfigureAwait(false);
        }
    }
}
