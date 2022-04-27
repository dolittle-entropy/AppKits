using Common.Models;

namespace Common.PublicMessaging
{
    public interface IMessageConsumer
    {
        event AsyncEventHandler<PublicMessage> MessageReceived;
        Task StartConsuming(CancellationToken stoppingToken);
    }
}
