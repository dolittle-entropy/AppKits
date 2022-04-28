using Common.EventHandling;
using Dolittle.SDK.Events;

namespace Sample.Products.Products
{
    [EventType("8b4a6578-d3db-4042-a9d9-5d8dc578a037")]
    public class ProductCreated
    {
        public ProductCreated(Product? payload, string? issuedBy, bool byM3 = false)
        {
            Payload = payload;
            IssuedBy = issuedBy;
            ByM3 = byM3;
        }

        public Product? Payload { get; }
        public string? IssuedBy { get; }
        public bool ByM3 { get; set; }

    }

    [EventType("c56c32be-9e0b-46da-b555-6fb7239e2aff")]
    public class ProductNameChangedByM3 : ValueChangeEvent<Guid, string>
    {
        public ProductNameChangedByM3(Guid id, string oldValue, string newValue, string? issuedBy) 
            : base(id, oldValue, newValue, issuedBy)
        {
        }
    }

    [EventType("60c66140-a43f-44a6-ba3f-5b9d5b1c3bfa")]
    public class ProductNameChangedByUser : ValueChangeEvent<Guid, string>
    {
        public ProductNameChangedByUser(Guid id, string oldValue, string newValue, string? issuedBy) 
            : base(id, oldValue, newValue, issuedBy)
        {
        }
    }

    [EventType("e71b83c6-a78b-4056-96f8-83523ab6e914")]
    public class ProductSKUChangedByM3 : ValueChangeEvent<Guid, string>
    {
        public ProductSKUChangedByM3(Guid id, string oldValue, string newValue, string? issuedBy) 
            : base(id, oldValue, newValue, issuedBy)
        {
        }
    }

    [EventType("e065e1b3-96f3-43d3-845a-4df2166e3a72")]
    public class ProductSKUChangedByUser : ValueChangeEvent<Guid, string>
    {
        public ProductSKUChangedByUser(Guid id, string oldValue, string newValue, string? issuedBy) 
            : base(id, oldValue, newValue, issuedBy)
        {
        }
    }
}
