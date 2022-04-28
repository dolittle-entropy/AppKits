using Common.Extensions;
using Common.GraphQL;
using Common.Models;
using Common.PublicMessaging;
using Common.Repositories;
using Common.Validation;
using Dolittle.SDK;
using ILogger = Serilog.ILogger;

namespace Sample.Products.Products
{
    public class UpsertProductFromM3Processor : PublicMessageProcessor<Product, UpsertProductFromM3>
    {
        readonly ILogger _log;

        public UpsertProductFromM3Processor(
            IValidator<Product> entityValidator, 
            IBusinessValidator<UpsertProductFromM3> businessValidator, 
            IDolittleClient dolittleClient, 
            INotificationPublisher notificationPublisher, 
            ICollectionService collectionService) 
            : base(entityValidator, businessValidator, dolittleClient, notificationPublisher, collectionService)
        {
            _log = Serilog.Log.ForContext<UpsertProductFromM3Processor>();
        }

        public override string ForMessageType => "UpsertProduct";

        public override async Task<bool> Process(UpsertProductFromM3 command, CancellationToken cancellationToken = default)
        {
            if (!await CommandIsValid(command, cancellationToken))
                return false;

            _log.Enter(this, command.Payload!.Id.ToString());

            return await DoOnAggregate<ProductsAggregate>(
                eventSourceId: command.Payload!.Id.ToString(),
                command: command,
                aggregateAction: act => act.Process(command))
                .ConfigureAwait(false);
        }

        public override async Task<bool> ProcessMessage(PublicMessage publicMessage)
        {            
            return await Process(new UpsertProductFromM3
            {
                TenantId = publicMessage.GetTenantId(),
                BrokerOffset = publicMessage.BrokerOffset,
                Payload = publicMessage.AsPayload<Product>()
            });
        }
    }
}
