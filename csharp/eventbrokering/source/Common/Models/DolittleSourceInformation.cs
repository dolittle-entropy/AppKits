namespace Common.Models
{
    public record DolittleSourceInformation
    {
        public string? EventHandler { get; init; }

        public Guid EventHandlerId { get; init; }

        public long SequenceNumber { get; init; }
    }
}
