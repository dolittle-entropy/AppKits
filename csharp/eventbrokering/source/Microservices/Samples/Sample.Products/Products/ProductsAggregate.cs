using Dolittle.SDK.Aggregates;
using Dolittle.SDK.Events;

namespace Sample.Products.Products
{
    [AggregateRoot("3d2b7cc5-480c-491a-8229-50bc47dcf4f7")]
    public class ProductsAggregate : AggregateRoot
    {
        Product? _currentProduct;
        string? _lastModifiedBy;
        bool _isDeleted;

        public ProductsAggregate(EventSourceId eventSourceId) : base(eventSourceId)
        {
            _isDeleted = false;
            _lastModifiedBy = null;
            _currentProduct = null;
        }

        public void Process(UpsertProductFromM3 command)
        {
            if (_isDeleted)
                return;

            if(_currentProduct is null)            
                Apply(new ProductCreated(command.Payload, command.IssuedBy, byM3:true));                
            else
                DetectChangesAndApplyEvents(command, byM3: true);                   
        }

        private void DetectChangesAndApplyEvents(UpsertProductFromM3 command, bool byM3 = false)
        {            
            if (command.Payload is null || _currentProduct is null)
                return;

            if(_currentProduct.Name != command.Payload.Name)
                if(byM3) Apply(new ProductNameChangedByM3(command.Payload.Id, _currentProduct.Name!, command.Payload.Name!, "M3"));
                else     Apply(new ProductNameChangedByUser(command.Payload.Id, _currentProduct.Name!, command.Payload.Name!, command.IssuedBy));

            if(_currentProduct.SKU != command.Payload.SKU)
                if(byM3) Apply(new ProductSKUChangedByM3(command.Payload.Id, _currentProduct.SKU!, command.Payload.SKU!, "M3"));
                else     Apply(new ProductSKUChangedByUser(command.Payload.Id, _currentProduct.SKU!, command.Payload.SKU!, command.IssuedBy));
        }

        void On(ProductCreated evt)
        {
            _currentProduct = evt.Payload;
            _lastModifiedBy = evt.IssuedBy;
            _isDeleted = false;
        }

        void On(ProductNameChangedByM3 evt)
        {
            if(_currentProduct is not null)
            {
                _currentProduct.Name = evt.NewValue;
                _currentProduct.LastModifiedBy = evt.IssuedBy;                
            }
        }

        void On(ProductNameChangedByUser evt)
        {
            if (_currentProduct is not null)
            {
                _currentProduct.Name = evt.NewValue;
                _currentProduct.LastModifiedBy = evt.IssuedBy;                
            }
        }

        void On(ProductSKUChangedByM3 evt)
        {
            if (_currentProduct is not null)
            {
                _currentProduct.SKU = evt.NewValue;
                _currentProduct.LastModifiedBy = evt.IssuedBy;
            }
        }

        void On(ProductSKUChangedByUser evt)
        {
            if (_currentProduct is not null)
            {
                _currentProduct.SKU = evt.NewValue;
                _currentProduct.LastModifiedBy = evt.IssuedBy;
            }
        }

    }
}