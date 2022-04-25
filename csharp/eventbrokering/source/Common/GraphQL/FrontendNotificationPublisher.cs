using HotChocolate.Subscriptions;
using Serilog;

namespace Common.GraphQL
{
    /// <summary>
    /// Publishes notifications to the frontend using a common structure
    /// </summary>
    public interface INotificationPublisher
    {
        Task Publish(FrontendNotification msg, CancellationToken cancellationToken = default);
    }

    public class FrontendNotificationPublisher : INotificationPublisher
    {
        readonly ILogger _log;
        readonly ITopicEventSender _topicEventSender;

        public FrontendNotificationPublisher(ILogger log, ITopicEventSender topicEventSender)
        {
            _log = log.ForContext<FrontendNotificationPublisher>();
            _topicEventSender = topicEventSender;
        }

        public async Task Publish(FrontendNotification msg, CancellationToken cancellationToken = default)
        {
            await _topicEventSender.SendAsync(nameof(FrontendSubscriptions.NotificationFromBackend), msg);
        }
    }

    public class FrontendSubscriptions
    {
        [Subscribe]
        [UseFiltering]
        public FrontendNotification NotificationFromBackend([EventMessage] FrontendNotification notification) => notification;
    }
}
