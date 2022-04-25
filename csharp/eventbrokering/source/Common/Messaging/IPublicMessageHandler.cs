namespace Common.Messaging
{
    public interface IPublicMessageHandler
    {
        void StartProcessing(CancellationToken cancellationToken = default);
    }
}
