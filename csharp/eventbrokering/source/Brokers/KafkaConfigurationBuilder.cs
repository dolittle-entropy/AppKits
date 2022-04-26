using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Serilog;
using System.Text;

namespace Brokers
{
    public static class KafkaConfigurationBuilder
    {
        public static ConsumerConfig BuildSubscriberConfiguration(IConfigurationSection kafka, ILogger log)
        {
            DisplayConfigurationValuesForKafka(kafka, log);
            var groupId = kafka["GroupId"];
            if (Environment.GetEnvironmentVariable("KAFKA__GROUP") is { } groupIdAdd)
            {
                groupId += $"-{groupIdAdd}";
                log.Information($"Kafka Consumer with id '{groupId}' starting");
            }

            return new ConsumerConfig
            {
                GroupId = groupId,
                EnableAutoOffsetStore = false,
                EnableAutoCommit = true,
                BootstrapServers = kafka["BrokerUrl"],
                AutoOffsetReset = AutoOffsetReset.Earliest,

                SecurityProtocol = SecurityProtocol.Ssl,
                SslCaLocation = kafka.GetSection("Ssl")["Authority"],
                SslCertificateLocation = kafka.GetSection("Ssl")["Certificate"],
                SslKeyLocation = kafka.GetSection("Ssl")["Key"],
            };
        }

        public static ProducerConfig BuildPublisherConfiguration(IConfigurationSection kafka, ILogger log)
        {
            DisplayConfigurationValuesForKafka(kafka, log);

            return new ProducerConfig
            {
                ClientId = kafka["GroupId"],
                BootstrapServers = kafka["BrokerUrl"],

                SecurityProtocol = SecurityProtocol.Ssl,
                SslCaLocation = kafka.GetSection("Ssl")["Authority"],
                SslCertificateLocation = kafka.GetSection("Ssl")["Certificate"],
                SslKeyLocation = kafka.GetSection("Ssl")["Key"],
                EnableIdempotence = true,

                QueueBufferingMaxKbytes = 1_000
            };
        }

        private static void DisplayConfigurationValuesForKafka(IConfigurationSection kafka, ILogger log)
        {
            var stringBuilder = new StringBuilder(2048);
            stringBuilder.AppendLine("Kafka Subscriber Environment values:");
            stringBuilder.AppendLine("------------------------------------");
            stringBuilder.AppendLine($"Kafka__GroupId: {kafka["GroupId"]}");
            stringBuilder.AppendLine($"Kafka__BrokerUrl: {kafka["BrokerUrl"]}");
            stringBuilder.AppendLine($"Kafka__Ssl__Authority: {kafka.GetSection("Ssl")["Authority"]}");
            stringBuilder.AppendLine($"Kafka__Ssl__Certificate: {kafka.GetSection("Ssl")["Certificate"]}");
            stringBuilder.AppendLine($"Kafka__Ssl__Key: {kafka.GetSection("Ssl")["Key"]}");
            stringBuilder.AppendLine($"Kafka__Ssl__Key: {kafka.GetSection("Ssl")["Key"]}");

            stringBuilder.AppendLine($"Kafka__InputTopic   : {kafka["InputTopic"]}");
            stringBuilder.AppendLine($"Kafka__CommandTopic : {kafka["CommandTopic"]}");
            stringBuilder.AppendLine($"Kafka__ReceiptsTopic: {kafka["ReceiptsTopic"]}");

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
            stringBuilder.AppendLine("------------------------------------");

            log.Information(stringBuilder.ToString());
        }
    }
}
