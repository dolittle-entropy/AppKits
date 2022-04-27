using Common.Extensions;
using Dolittle.SDK.Aggregates;
using Dolittle.SDK.Events;
using ILogger = Serilog.ILogger;

namespace Sample.Orders.Orders
{
    [AggregateRoot("d285cd58-4c96-41cd-a0ba-0f4a71ed6249")]
    public class OrdersAggregate : AggregateRoot
    {
        readonly ILogger _log;
        Order? _currentOrder;
        bool _isDeleted;

        public OrdersAggregate(EventSourceId eventSourceId) : base(eventSourceId)
        {
            _log = Serilog.Log.ForContext<OrdersAggregate>();
        }

        public void Process(CreateOrder command)
        {
            _log.Enter(this, $"{nameof(CreateOrder)} invoked");

            if(_currentOrder is null)
            {
                Apply(new OrderCreated(command.Payload!));
            }
        }

        public void Process(DeleteOrder command)
        {
            if (_currentOrder is null && _isDeleted)
                return;

            Apply(new OrderDeleted(command.Payload!.Id, command.IssuedBy!));
        }

        void On(OrderCreated evt)
        {
            _currentOrder = evt.NewOrder;
        }

        void On(OrderDeleted evt)
        {
            _isDeleted = true;
            _currentOrder = null;
        }
    }
}