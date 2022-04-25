using Common.Models;

namespace Common.Messaging
{
    public interface IMessageConsumer
    {
        event AsyncEventHandler<PublicMessage> MessageReceived;
        Task StartConsuming(CancellationToken stoppingToken);
    }
}
