namespace Common.PublicMessaging
{
    public interface IMessagePublisher
    {
        Task<bool> Publish(PublicCommand publicCommand, CancellationToken token = default);
    }
}
