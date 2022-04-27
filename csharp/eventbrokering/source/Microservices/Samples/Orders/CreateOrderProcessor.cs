using Common.Commands;
using Common.Extensions;
using Common.GraphQL;
using Common.Repositories;
using Common.Validation;
using Dolittle.SDK;
using ILogger = Serilog.ILogger;

namespace Sample.Orders.Orders
{
    public class CreateOrderProcessor : CommandProcessor<Order, CreateOrder>
    {
        readonly ILogger _log;

        public CreateOrderProcessor(
            ILogger log,
            IValidator<Order> entityValidator, 
            IBusinessValidator<CreateOrder> businessValidator, 
            IDolittleClient dolittleClient, 
            INotificationPublisher notificationPublisher, 
            ICollectionService collectionService) 
            : base(entityValidator, businessValidator, dolittleClient, notificationPublisher, collectionService)
        {
            _log = log.ForContext<CreateOrderProcessor>();
        }

        public override async Task<bool> Process(CreateOrder command, CancellationToken cancellationToken = default)
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
