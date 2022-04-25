using Common.Models;
using Common.Processing;

namespace Common.Messaging
{
    public delegate Task AsyncEventHandler<TEventArgs>(object? sender, TEventArgs e);

    public class PublicMessageHandler : IPublicMessageHandler
    {
        readonly List<Guid> _approvedTenants;        
        private readonly IMessageConsumer _messageConsumer;
        private readonly IReceiptConsumer _receiptConsumer;
        private readonly Dictionary<string, IPublicMessageProcessor> _processors;
        private readonly Dictionary<string, IReceiptProcessor> _receiptProcessors;

        public PublicMessageHandler(
            IMessageConsumer messageConsumer,
            IReceiptConsumer receiptConsumer,
            List<Guid> approvedTenants,
            Dictionary<string, IPublicMessageProcessor> processors,
            Dictionary<string, IReceiptProcessor> receiptProcessors)
        {
            _approvedTenants = approvedTenants;
            _messageConsumer = messageConsumer;
            _receiptConsumer = receiptConsumer;
            _processors = processors;
            _receiptProcessors = receiptProcessors;
        }

        public void StartProcessing(CancellationToken cancellationToken = default)
        {
            _messageConsumer.MessageReceived -= async (sender, message) => await HandleMessageReceived(message, cancellationToken);
            _messageConsumer.MessageReceived += async (sender, message) => await HandleMessageReceived(message, cancellationToken);

            _receiptConsumer.ReceiptReceived -= async (sender, message) => await HandleReceiptMessage(message, cancellationToken);
            _receiptConsumer.ReceiptReceived += async (sender, message) => await HandleReceiptMessage(message, cancellationToken);

            _messageConsumer.StartConsuming(cancellationToken);
            _receiptConsumer.StartConsuming(cancellationToken);
        }

        private async Task HandleReceiptMessage(PublicReceipt message, CancellationToken cancellationToken)
        {
            var messageType = message.Command;
            if (string.IsNullOrEmpty(messageType))
                return;

            if (_receiptProcessors.ContainsKey(messageType) && _receiptProcessors[messageType] is { } processor)
            {
                var success = await processor
                    .ProcessReceipt(message, cancellationToken)
                    .ConfigureAwait(false);
            }
        }

        public async Task HandleMessageReceived(PublicMessage message, CancellationToken cancellationToken)
        {
            var tenant = message.GetTenantId();

            if (message is null)
                return;

            var messageType = message.Metadata!.MessageType!;

            if (tenant == Guid.Empty)
                return;

            if (!_approvedTenants.Contains(tenant))
                return;

            if (_processors.ContainsKey(messageType))
            {
                var success = await _processors[messageType!]
                    .ProcessMessage(message!)
                    .ConfigureAwait(false);
            }
        }
    }
}
