using Common.Commands;
using Common.Rejections;

namespace Sample.Orders.Orders
{
    public class CreateOrder : Rejection, ICommand<Order>
    {
        public Guid TenantId { get; set; }
        public string? IssuedBy { get; set; }

        public Order? Payload { get; set; }
    }
}
