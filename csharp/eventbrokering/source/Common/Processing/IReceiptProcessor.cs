using Common.Models;

namespace Common.Processing
{
    public interface IReceiptProcessor
    {
        string ForReceiptType { get; }
        Task<bool> ProcessReceipt(PublicReceipt receipt, CancellationToken cancellationToken = default);
    }
}
