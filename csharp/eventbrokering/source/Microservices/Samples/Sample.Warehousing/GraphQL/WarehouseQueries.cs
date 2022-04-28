using Common.Constants;
using Common.GraphQL;
using Common.Repositories;
using HotChocolate;
using HotChocolate.Data;
using MongoDB.Driver;
using Sample.Warehousing.Warehouses;
using ILogger = Serilog.ILogger;

namespace Sample.Warehousing.GraphQL
{
    [GraphQLName(nameof(WarehouseQueries)), GraphQLDescription("Queries targeting the Bounded context 'Warehouse'")]
    public class WarehouseQueries : BaseGQLQuery
    {
        readonly ILogger _log;
        readonly IMongoCollection<Warehouse> _warehouses;        

        public WarehouseQueries(ILogger log, ICollectionService collectionService)
            : base(collectionService, BoundedContexts.Warehouses)
        {
            _log = log.ForContext<WarehouseQueries>();
            _warehouses = collectionService.GetMongoCollection<Warehouse>(CollectionNames.Warehouses)!;
        }

        public IExecutable<Warehouse> GetWarehouses()
            => _warehouses.AsExecutable();

    }
}