using Common.Constants;
using Common.EventHandling;
using Common.GraphQL;
using Common.PublicMessaging;
using Common.Repositories;
using Dolittle.SDK.Events.Handling;
using ILogger = Serilog.ILogger;

namespace Sample.Products.Products
{
    [EventHandler("25c4a2f0-fc70-47ad-adf5-fd74762ad71f")]
    public class ProductEventHandler : TransactionalEventHandler<Product>
    {
        readonly ILogger _log;

        public ProductEventHandler(
            ILogger log, 
            INotificationPublisher notificationPublisher, 
            IMessagePublisher messagePublisher, 
            ICollectionService collectionService) 
            : base(log, notificationPublisher, messagePublisher, collectionService, CollectionNames.Products)
        {
            _log = log.ForContext<ProductEventHandler>();    
        }
    }
}