using Common.Constants;
using Common.GraphQL;
using Common.Repositories;
using HotChocolate;
using HotChocolate.Data;
using Sample.Orders.Orders;
using ILogger = Serilog.ILogger;

namespace Sample.Orders.GraphQL
{
    [GraphQLName(nameof(OrderQueries)), GraphQLDescription("Queries related to the bounded context 'Orders'")]
    public class OrderQueries : BaseGQLQuery
    {
        readonly ILogger _log;
        readonly IRepository<Order> _orders;

        public OrderQueries(ILogger log, ICollectionService collectionService) 
            : base(collectionService, BoundedContexts.Orders)
        {
            _log = log.ForContext<OrderQueries>();
            _orders = collectionService.GetRepository<Order>(CollectionNames.Orders);
        }

        [UseProjection, UseFiltering, UseSorting]
        public async Task<IEnumerable<Order>?> GetOrdersInRange(DateTime from, DateTime to)
            => await _orders.GetAll(x => x.Date >= from && x.Date <= to);
    }
}
