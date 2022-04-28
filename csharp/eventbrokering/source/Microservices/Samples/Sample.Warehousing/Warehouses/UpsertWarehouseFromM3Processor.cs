using Common.Commands;
using Common.Constants;
using Common.Extensions;
using Common.GraphQL;
using Common.Models;
using Common.PublicMessaging;
using Common.Repositories;
using Common.Validation;
using Dolittle.SDK;
using ILogger = Serilog.ILogger;

namespace Sample.Warehousing.Warehouses
{
    public class UpsertWarehouseFromM3Processor : PublicMessageProcessor<Warehouse, UpsertWarehouseFromM3>
    {
        readonly ILogger _log;
        readonly IRepository<Warehouse> _warehouses;

        public UpsertWarehouseFromM3Processor(
            IValidator<Warehouse> entityValidator, 
            IBusinessValidator<UpsertWarehouseFromM3> businessValidator, 
            IDolittleClient dolittleClient, 
            INotificationPublisher notificationPublisher, 
            ICollectionService collectionService) 
            : base(entityValidator, businessValidator, dolittleClient, notificationPublisher, collectionService)
        {
            _log = Serilog.Log.ForContext<UpsertWarehouseFromM3Processor>();
            _warehouses = collectionService.GetRepository<Warehouse>(CollectionNames.Warehouses);
        }

        public override string ForMessageType => "UpsertWarehouse";

        public override async Task<bool> Process(UpsertWarehouseFromM3 command, CancellationToken cancellationToken = default)
        {
            _log.Enter(this, nameof(UpsertWarehouseFromM3));

            if (!await CommandIsValid(command, cancellationToken))
                return false;

            var warehouseToInsert = command.Payload;
            if (warehouseToInsert is null)
                return false;

            return await _warehouses.Upsert(x => x.Id == warehouseToInsert.Id, warehouseToInsert, cancellationToken) is Warehouse
                ? true
                : false;
        }

        public override async Task<bool> ProcessMessage(PublicMessage publicMessage)
        {
            return await Process(new UpsertWarehouseFromM3
            {
                TenantId = publicMessage.GetTenantId(),
                BrokerOffset = publicMessage.BrokerOffset,
                Payload = publicMessage.AsPayload<Warehouse>()
            }).ConfigureAwait(false);
        }
    }
}
