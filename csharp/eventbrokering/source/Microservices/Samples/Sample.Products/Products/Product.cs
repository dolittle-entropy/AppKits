using Common.Models;

namespace Sample.Products.Products
{
    public class Product : IEntity
    {
        public Guid Id { get; set; }

        public bool? IsSynchronized { get; set; }
    }
}
