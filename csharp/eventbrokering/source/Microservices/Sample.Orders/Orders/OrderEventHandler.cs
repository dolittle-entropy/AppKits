using Common.Constants;
using Common.EventHandling;
using Common.Extensions;
using Common.GraphQL;
using Common.Repositories;
using Dolittle.SDK.Events;
using Dolittle.SDK.Events.Handling;
using ILogger = Serilog.ILogger;

namespace Sample.Orders.Orders
{
    [EventHandler("68906dd7-3422-4e3f-bec7-db6e4866aec6")]
    public class OrderEventHandler : NotificationEventHandler<Order>
    {
        readonly ILogger _log;

        public OrderEventHandler(ILogger log, INotificationPublisher notificationPublisher, ICollectionService collectionService)
            : base(log, notificationPublisher, collectionService, CollectionNames.Orders)
        {
            _log = log.ForContext<OrderEventHandler>();

        }

        public async Task Handle(OrderCreated evt, EventContext eventContext)
        {
            var commandName = nameof(OrderCreated);
            evt.NewOrder.IsSynchronized = true; // ensure we're synchronized at this stage
            evt.NewOrder.LastModified = DateTime.UtcNow;

            if(await CreateReadModel(order => order.Id == evt.NewOrder!.Id, evt.NewOrder) is { } created)
            {
                await SendUserConfirmation(created.LastModifiedBy!, created, commandName);
            }
            else
            {
                await SendErrorToUser(evt.NewOrder.LastModifiedBy!, commandName, evt, commandName);
            }
            _log.Leave(this, $"Handle({commandName} {nameof(evt)}) finished");
        }
    }
}
