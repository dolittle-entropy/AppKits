using Common.Extensions;
using Common.Models;
using Common.PublicMessaging;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace Brokers
{
    public class KafkaReceiptConsumer : IReceiptConsumer
    {
        public event AsyncEventHandler<PublicReceipt>? ReceiptReceived;

        readonly ILogger _log;
        readonly ConsumerConfig _kafkaConfiguration;
        readonly string _receiptTopic;

        public KafkaReceiptConsumer(IConfiguration config, ILogger log)
        {
            _log = log.ForContext<KafkaReceiptConsumer>();
            var kafkaConfigurationSection = config.GetSection("Kafka");
            _receiptTopic = kafkaConfigurationSection["ReceiptsTopic"];
            _kafkaConfiguration = KafkaConfigurationBuilder.BuildSubscriberConfiguration(kafkaConfigurationSection, _log);
        }

        public async Task StartConsuming(CancellationToken cancellationToken)
        {
            _log.Enter(this, $"Listening for receipts");
            try
            {
                using var consumer = new ConsumerBuilder<Ignore, PublicReceipt>(_kafkaConfiguration)
                    .SetValueDeserializer(new PublicReceiptDeserializer())
                    .SetPartitionsAssignedHandler((c, partitions) =>
                    {
                        partitions.Select(partition => new TopicPartitionOffset(partition, Offset.Beginning));
                    })
                    .Build();

                consumer.Subscribe(_receiptTopic);
                while (!cancellationToken.IsCancellationRequested)
                {
                    await Task.Run(async () =>
                    {
                        try
                        {
                            if (consumer.Consume(cancellationToken) is ConsumeResult<Ignore, PublicReceipt> receiptResult)
                            {
                                if (receiptResult.Message.Value is PublicReceipt)
                                {
                                    if (ReceiptReceived != null)
                                    {
                                        await ReceiptReceived.Invoke(this, receiptResult.Message.Value);
                                    }
                                }

                                consumer.StoreOffset(receiptResult);
                            }
                        }
                        catch (Exception ex)
                        {
                            _log.Warn(this, $"{ex.Message}{Environment.NewLine}{ex.StackTrace}");
                        }
                    });
                };
            }
            catch (Exception ex)
            {
                _log.Fail(this, $"{ex.Message}{Environment.NewLine}{ex.StackTrace}");
            }
        }
    }
}
