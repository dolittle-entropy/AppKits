using Common.Messaging;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Serilog;
using System.Text;

namespace Brokers
{
    public class KafkaPublisher : IMessagePublisher
    {
        readonly ILogger _log;
        readonly ProducerConfig _kafkaConfiguration;

        readonly string _outputTopic;

        public KafkaPublisher(IConfiguration config)
        {
            _log = Log.ForContext<KafkaPublisher>();
            var configSection = config.GetSection("Kafka");
            _kafkaConfiguration = KafkaConfigurationBuilder.BuildPublisherConfiguration(configSection, _log);
            _outputTopic = configSection["CommandTopic"];
        }

        public async Task<bool> Publish(PublicCommand publicCommand, CancellationToken token = default)
        {
            var json = JsonConvert.SerializeObject(publicCommand);

            var producer = new ProducerBuilder<string, string>(_kafkaConfiguration).Build();
            var message = new Message<string, string>
            {
                Key = publicCommand.Metadata!.MessageType!,
                Value = json
            };
            try
            {
                var producedResult = await producer.ProduceAsync(_outputTopic, message, token).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _log.Error(ex, $"Failed to transmit command to M3 Connector");
                return true;
            }
            return true;
        }

        ProducerConfig BuildConfiguration(IConfigurationSection kafka)
        {
            DisplayConfigurationValuesForKafka(kafka);
            var groupId = kafka["GroupId"];
            if (Environment.GetEnvironmentVariable("KAFKA__GROUP") is { } groupIdAdd)
            {
                groupId += $"-{groupIdAdd}";
                _log.Information($"Kafka Consumer with id '{groupId}' starting");
            }

            return new ProducerConfig
            {
                BootstrapServers = kafka["BrokerUrl"],
                SecurityProtocol = SecurityProtocol.Ssl,
                SslCaLocation = kafka.GetSection("Ssl")["Authority"],
                SslCertificateLocation = kafka.GetSection("Ssl")["Certificate"],
                SslKeyLocation = kafka.GetSection("Ssl")["Key"],
                MessageMaxBytes = 25_000,
                ReceiveMessageMaxBytes = 55_000
            };
        }

        private void DisplayConfigurationValuesForKafka(IConfigurationSection kafka)
        {
            var stringBuilder = new StringBuilder(2048);
            stringBuilder.AppendLine("Kafka Command Environment values:");
            stringBuilder.AppendLine("---------------------------------");
            stringBuilder.AppendLine($"Kafka__GroupId: {kafka["GroupId"]}");
            stringBuilder.AppendLine($"Kafka__BrokerUrl: {kafka["BrokerUrl"]}");
            stringBuilder.AppendLine($"Kafka__Ssl__Authority: {kafka.GetSection("Ssl")["Authority"]}");
            stringBuilder.AppendLine($"Kafka__Ssl__Certificate: {kafka.GetSection("Ssl")["Certificate"]}");
            stringBuilder.AppendLine($"Kafka__Ssl__Key: {kafka.GetSection("Ssl")["Key"]}");

            if (File.Exists(kafka.GetSection("Ssl")["Authority"]))
            {
                stringBuilder.AppendLine($"PEM file found: YES");
            }
            else
            {
                stringBuilder.AppendLine($"PEM file found: NO");
            }

            if (File.Exists(kafka.GetSection("Ssl")["Certificate"]))
            {
                stringBuilder.AppendLine($"Certificate file found: YES");
            }
            else
            {
                stringBuilder.AppendLine($"Certificate file found: NO");
            }

            if (File.Exists(kafka.GetSection("Ssl")["Key"]))
            {
                stringBuilder.AppendLine($"Key file found: YES");
            }
            else
            {
                stringBuilder.AppendLine($"Key file found: NO");
            }
            stringBuilder.AppendLine("---------------------------------");

            _log.Information(stringBuilder.ToString());
        }
    }
}
