using Dolittle.SDK.Events;

namespace Sample.Orders.Orders
{
    [EventType("4c61b9f0-9745-444d-9d02-350e3efe6453")]
    public record OrderCreated(Order NewOrder);

    [EventType("4555c1f8-9d36-4493-ab18-52cfffd88bbd")]
    public record OrderDeleted(Guid OrderId, string issuedBy);
}