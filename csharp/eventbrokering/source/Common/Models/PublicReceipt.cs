namespace Common.Models
{
    public record PublicReceipt
    {
        public string? Command { get; set; }

        public string? CorrelationId { get; set; }

        public bool? Success { get; set; }
        public Error? Error { get; set; }

        public object? Payload { get; set; }
    }
}
