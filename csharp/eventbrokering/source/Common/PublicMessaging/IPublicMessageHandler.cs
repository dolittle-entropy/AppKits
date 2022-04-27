namespace Common.PublicMessaging
{
    public interface IPublicMessageHandler
    {
        void StartProcessing(CancellationToken cancellationToken = default);
    }
}
