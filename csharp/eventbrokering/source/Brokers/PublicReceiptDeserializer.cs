using Common.Models;
using Confluent.Kafka;
using System.Text.Json;

namespace Brokers
{
    public class PublicReceiptDeserializer : IDeserializer<PublicReceipt>
    {
        public PublicReceipt Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
        {
            if (isNull || data.Length == 0)
                return new PublicReceipt();

            var options = new JsonSerializerOptions
            {
                MaxDepth = 10,
                PropertyNameCaseInsensitive = true
            };
            return JsonSerializer.Deserialize<PublicReceipt>(data, options) is PublicReceipt receipt
                ? receipt
                : new PublicReceipt();
        }
    }
}