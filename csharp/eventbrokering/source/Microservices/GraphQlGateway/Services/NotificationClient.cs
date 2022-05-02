using Common.GraphQL;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using StackExchange.Redis;
using System.Threading;
using System.Threading.Tasks;

namespace GraphQlGateway.Services
{
    public class NotificationClient : IHostedService
    {
        readonly ISubscriber _subscriber;
        readonly INotificationPublisher _notificationPublisher;

        public NotificationClient(ConnectionMultiplexer connectionMultiplexer, INotificationPublisher notificationPublisher)
        {
            _subscriber = connectionMultiplexer.GetSubscriber();
            _notificationPublisher = notificationPublisher;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Subscribe<FrontendNotification>("NotificationFromBackend");

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
            => Task.CompletedTask;

        void Subscribe<TPayload>(string topic)
        {
            _subscriber.Subscribe(topic, async (chn, msg) =>
            {
                await Handle(chn, msg, topic);
            });
        }

        async ValueTask Handle(RedisChannel chn, RedisValue msg, string topic)
        {
            var payload = ConvertStringToPayload<FrontendNotification>(msg.ToString());
            if (payload is { })
            {
                await _notificationPublisher.Publish(payload);
            }
        }

        TPayload? ConvertStringToPayload<TPayload>(string value)
        {
            try
            {
                return JsonConvert.DeserializeObject<TPayload>(value);
            }
            catch
            {
                /* Are we worried yet? */
            }
            return default;
        }
    }
}
