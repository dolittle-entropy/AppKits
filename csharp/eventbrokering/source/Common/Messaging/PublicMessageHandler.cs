using Common.Extensions;
using Common.Models;
using Common.Processing;
using Serilog;

namespace Common.Messaging
{
    public delegate Task AsyncEventHandler<TEventArgs>(object? sender, TEventArgs e);

    public class PublicMessageHandler : IPublicMessageHandler
    {
        readonly ILogger _log;
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
            _log = Log.ForContext<PublicMessageHandler>();

            _messageConsumer = messageConsumer;
            _receiptConsumer = receiptConsumer;
            _approvedTenants = approvedTenants;
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
            {
                _log.Warn(this, $"Received a receipt with an empty command");
                return;
            }

            if (_receiptProcessors.ContainsKey(messageType) && _receiptProcessors[messageType] is { } processor)
            {
                var success = await processor
                    .ProcessReceipt(message, cancellationToken)
                    .ConfigureAwait(false);

                if (success)
                    _log.Leave(this, $"Completed processing receipt for {messageType}: '{message.CorrelationId}'");
                else
                    _log.Fail(this, $"Failed to process receipt for {messageType}: '{message.CorrelationId}'");

            }
        }

        public async Task HandleMessageReceived(PublicMessage message, CancellationToken cancellationToken)
        {
            var tenant = message.GetTenantId();

            if (message is null)
            {
                _log.Warn(this, $"Received a receipt with an empty command");
                return;
            }

            if (tenant == Guid.Empty)
            {
                _log.Warn(this, $"No tenancy in message. Will not process\n");
                return;
            }

            if (!_approvedTenants.Contains(tenant))
            {
                _log.Warn(this, $"Tenant {tenant} is not recognized. Will not process\n\n");
                return;
            }

            var messageType = message.Metadata!.MessageType!;

            if (_processors.ContainsKey(messageType))
            {
                var success = await _processors[messageType!]
                    .ProcessMessage(message!)
                    .ConfigureAwait(false);

                _log.Leave(this, $"Completed processing Message: '{message.Metadata?.MessageType}'");
            }
        }
    }
}
