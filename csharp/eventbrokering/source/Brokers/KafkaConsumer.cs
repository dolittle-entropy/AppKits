using Common.Extensions;
using Common.Models;
using Common.PublicMessaging;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace Brokers;

public class KafkaConsumer : IMessageConsumer
{
    public event AsyncEventHandler<PublicMessage>? MessageReceived;

    readonly ILogger _log;
    readonly ConsumerConfig _kafkaConfiguration;

    // Debugging things
    readonly List<string> _ignoredMessageTypes;
    private readonly long _offsetStart;

    string _inputTopic { get; }

    public KafkaConsumer(IConfiguration config)
    {
        _log = Log.ForContext<KafkaConsumer>();

        _ignoredMessageTypes = config["IGNORE_MESSAGETYPES"] is string data
            ? data.Split(',').Select(e => e.Trim()).ToList()
            : new List<string>();

        _offsetStart = config["DEBUG_STARTONOFFSET"] is string stringValue
            ? long.Parse(stringValue)
            : 0;

        var kafkaConfigurationSection = config.GetSection("Kafka");
        _inputTopic = kafkaConfigurationSection["InputTopic"];
        _kafkaConfiguration = KafkaConfigurationBuilder.BuildSubscriberConfiguration(kafkaConfigurationSection, _log);
    }

    public ValueTask DisposeAsync()
    {
        _log.Enter(this, $"Broker listener shutting down..");

        return new ValueTask();
    }

    public async Task StartConsuming(CancellationToken stoppingToken)
    {
        _log.Enter(this, $"Started listening for broker messages");

        try
        {
            using var consumer = new ConsumerBuilder<Ignore, PublicMessage>(_kafkaConfiguration)
                .SetValueDeserializer(new PublicMessageDeserializer())
                .Build();

            SetConsumerOffsetStart(consumer);

            await Task.Run(async () =>
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    if (consumer.Consume(stoppingToken) is ConsumeResult<Ignore, PublicMessage> consumeResult)
                    {
                        if (consumeResult.Message.Value is PublicMessage publicMessage)
                        {
                            var offset = consumeResult.TopicPartitionOffset.Offset.Value;
                            publicMessage.BrokerOffset = offset.ToString();                            
                            if (MessageReceived != null)
                            {
                                await MessageReceived.Invoke(this, publicMessage);
                            }

                            var hostName = Environment.GetEnvironmentVariable("KEEPOFFSETS");
                            if (!string.IsNullOrEmpty(hostName) && hostName.Equals("Yes", StringComparison.InvariantCultureIgnoreCase))
                            {
                                consumer.StoreOffset(consumeResult.TopicPartitionOffset);
                            }
                        }
                    }
                    await Task.Yield();
                }
                consumer.Close();
            }, stoppingToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _log.Fail(this, ex.Message);
        }
    }

    private void SetConsumerOffsetStart(IConsumer<Ignore, PublicMessage> consumer)
    {
        switch (_offsetStart)
        {
            case 0:
                consumer.Subscribe(_inputTopic);
                break;
            case > 0:
                consumer.Assign(new TopicPartitionOffset(_inputTopic, 0, new Offset(_offsetStart)));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(_offsetStart));
        }
    }
}