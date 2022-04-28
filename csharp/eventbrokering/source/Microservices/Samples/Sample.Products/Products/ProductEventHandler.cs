using Common.Constants;
using Common.EventHandling;
using Common.Extensions;
using Common.GraphQL;
using Common.PublicMessaging;
using Common.Repositories;
using Dolittle.SDK.Events;
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

        public async Task Handle(ProductCreated evt, EventContext eventContext)
        {
            if (evt is null || evt.Payload is null)
                return;

            var creator = evt.ByM3 ? "M3" : evt.IssuedBy;
            var newProduct = evt.Payload with
            {
                CreatedBy = creator,
                Created = DateTime.UtcNow,
                LastModifiedBy = creator,
                LastModified = DateTime.UtcNow,
                IsSynchronized = true
            };

            if (await CreateReadModel(x => x.Id! == newProduct.Id, newProduct) is { } createdProduct)
            {
                await SendCreateNotificationToFrontend(createdProduct);
                _log.Leave(this, $"{nameof(ProductCreated)} id: {createdProduct.Id} created by {creator}");
                return;
            }

            if (!evt.ByM3)
            {
                await SendErrorToUser(
                    evt.IssuedBy!,
                    nameof(ProductCreated),
                    newProduct,
                    $"Failed to create read model of {nameof(Product)} with id: {newProduct.Id}");
            }
            _log.Fail(this, $"Unable to execute {nameof(ProductCreated)} id: {newProduct.Id} issued by {creator}");
        }

        public async Task Handle(ProductNameChangedByM3 evt, EventContext eventContext)
        {
            if (await Repository.GetOne(x => x.Id == evt.Id) is Product model)
            {
                model.Name = evt.NewValue;
                model.LastModifiedBy = evt.IssuedBy;
                model.LastModified = DateTime.UtcNow;
                model.IsSynchronized = true;
                if (await UpdateReadModel(x => x.Id == model.Id, model) is Product updatedProduct)
                    await SendCreateNotificationToFrontend(updatedProduct);
            }
            else
            {
                _log.Warn(this, $"Attempt to update {nameof(Product)} {evt.Id} but this record was not found");
            }
            _log.Leave(this, nameof(ProductNameChangedByM3));
        }

        public async Task Handle(ProductNameChangedByUser evt, EventContext eventContext)
        {
            if (await Repository.GetOne(x => x.Id == evt.Id) is Product model)
            {
                model.Name = evt.NewValue;
                model.LastModifiedBy = evt.IssuedBy;
                model.LastModified = DateTime.UtcNow;
                model.IsSynchronized = true;
                if (await UpdateReadModel(x => x.Id == model.Id, model) is Product updatedProduct)
                    await SendCreateNotificationToFrontend(updatedProduct);
                else
                    await SendErrorToUser(evt.IssuedBy!, nameof(ProductNameChangedByUser), evt, "Update failed");
            }
            else
            {
                _log.Warn(this, $"Attempt to update {nameof(Product)} {evt.Id} but this record was not found");
            }
            _log.Leave(this, nameof(ProductNameChangedByUser));
        }

        public async Task Handle(ProductSKUChangedByM3 evt, EventContext eventContext)
        {
            if (await Repository.GetOne(x => x.Id == evt.Id) is Product model)
            {
                model.SKU = evt.NewValue;
                model.LastModifiedBy = evt.IssuedBy;
                model.LastModified = DateTime.UtcNow;
                model.IsSynchronized = true;
                if (await UpdateReadModel(x => x.Id == model.Id, model) is Product updatedProduct)
                    await SendCreateNotificationToFrontend(updatedProduct);
            }
            else
            {
                _log.Warn(this, $"Attempt to update {nameof(Product)} {evt.Id} but this record was not found");
            }

            _log.Leave(this, nameof(ProductSKUChangedByM3));
        }

        public async Task Handle(ProductSKUChangedByUser evt, EventContext eventContext)
        {
            if (await Repository.GetOne(x => x.Id == evt.Id) is Product model)
            {
                model.SKU = evt.NewValue;
                model.LastModifiedBy = evt.IssuedBy;
                model.LastModified = DateTime.UtcNow;
                model.IsSynchronized = true;

                if (await UpdateReadModel(x => x.Id == model.Id, model) is Product updatedProduct)
                    await SendCreateNotificationToFrontend(updatedProduct);
                else
                    await SendErrorToUser(evt.IssuedBy!, nameof(ProductSKUChangedByUser), evt, "Update failed");
            }
            else
            {
                _log.Warn(this, $"Attempt to update {nameof(Product)} {evt.Id} but this record was not found");
            }
            _log.Leave(this, nameof(ProductSKUChangedByUser));
        }

    }
}