using Newtonsoft.Json;

namespace Common.Models
{
    public record PublicMessage
    {
        public Metadata? Metadata { get; init; }

        public string? BrokerOffset { get; set; }

        public object? Payload { get; init; }

        public TPayload? AsPayload<TPayload>() where TPayload : IEntity
        {
            if (Payload is { } payload)
            {
                var serialized = JsonConvert.SerializeObject(payload);
                return JsonConvert.DeserializeObject<TPayload>(serialized);
            }
            return default;
        }

        public Guid GetTenantId()
        {
            return Metadata?.Tenant?.Id ?? Guid.Empty;
        }
    }
}
