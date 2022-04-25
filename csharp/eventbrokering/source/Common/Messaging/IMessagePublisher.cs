namespace Common.Messaging
{
    public interface IMessagePublisher
    {
        Task<bool> Publish(PublicCommand publicCommand, CancellationToken token = default);
    }
}
