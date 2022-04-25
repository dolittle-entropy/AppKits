using Common.Models;

namespace Common.Messaging
{
    public interface IReceiptConsumer
    {
        event AsyncEventHandler<PublicReceipt> ReceiptReceived;
        Task StartConsuming(CancellationToken cancellationToken);
    }
}
