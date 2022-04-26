using Common.Models;
using Confluent.Kafka;
using System.Text.Json;

namespace Brokers
{
    public class PublicMessageDeserializer : IDeserializer<PublicMessage>
    {
        public PublicMessage Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
        {
            if (isNull || data.Length == 0)
                return new PublicMessage();

            var options = new JsonSerializerOptions
            {
                MaxDepth = 10,
                PropertyNameCaseInsensitive = true,
            };
            return JsonSerializer.Deserialize<PublicMessage>(data, options) is PublicMessage publicMessage
                ? publicMessage
                : new PublicMessage();
        }
    }
}
