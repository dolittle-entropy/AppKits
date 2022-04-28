using Common.Commands;
using Common.Rejections;

namespace Sample.Products.Products
{
    public class UpsertProductFromM3 : Rejection, ICommand<Product>
    {
        public Guid TenantId { get; set; }

        public string? IssuedBy { get; set; }

        public Product? Payload { get; set; }
    }
}
