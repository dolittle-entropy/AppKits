using Common.Models;

namespace Sample.Products.Products
{
    public record Product : IEntity
    {
        public Guid Id { get; set; }

        public string? Name { get; set; }

        public string? SKU { get; set; }

        public string? CreatedBy { get; set; }

        public DateTime Created { get; set; }

        public string? LastModifiedBy { get; set; }

        public DateTime LastModified { get; set; }

        public bool? IsSynchronized { get; set; }        
    }
}
