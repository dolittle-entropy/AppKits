namespace Common.Models
{
    public record Metadata
    {
        public string? MessageType { get; init; }
        public string? CorrelationId { get; set; }

        public string? SourceName { get; init; }

        public TenantInformation? Tenant { get; init; }

        public ApplicationInformation? Application { get; init; }

        public MicroserviceInformation? Microservice { get; init; }

        public DolittleSourceInformation? Extended { get; set; }
    }
}
