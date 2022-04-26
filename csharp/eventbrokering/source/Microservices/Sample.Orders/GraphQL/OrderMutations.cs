using Common.Constants;
using Common.Extensions;
using Common.Repositories;
using Dolittle.SDK;
using HotChocolate;
using Sample.Orders.Orders;
using ILogger = Serilog.ILogger;

namespace Sample.Orders.GraphQL
{
    public class OrderMutations
    {
        readonly ILogger _log;
        readonly IDolittleClient _dolittleClient;
        readonly IRepository<Order> _orders;

        public OrderMutations(ILogger log, IDolittleClient dolittleClient, ICollectionService collectionService)
        {
            _log = log.ForContext<OrderMutations>();
            _dolittleClient = dolittleClient;
            _orders = collectionService.GetRepository<Order>(CollectionNames.Orders);
        }

        public async Task<bool> CreateOrder(string? issuedBy, [Service] CreateOrderProcessor processor, CancellationToken token = default)
        {
            _log.Enter(this, $"{nameof(OrderMutations)}.{nameof(CreateOrder)}() invoked by {issuedBy ?? "unknown"}");

            var command = new CreateOrder
            {
                TenantId = _dolittleClient.Tenants.First().Id,
                Payload = new Order
                {
                    Id             = Guid.NewGuid(),
                    Date           = DateTime.UtcNow,
                    CreatedBy      = issuedBy,
                    LastModifiedBy = issuedBy,                    
                }
            };
            return await processor.Process(command, token);
        }
    }
}
