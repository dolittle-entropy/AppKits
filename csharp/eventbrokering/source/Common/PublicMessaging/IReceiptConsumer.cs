using Common.Models;

namespace Common.PublicMessaging
{
    public interface IReceiptConsumer
    {
        event AsyncEventHandler<PublicReceipt> ReceiptReceived;
        Task StartConsuming(CancellationToken cancellationToken);
    }
}
