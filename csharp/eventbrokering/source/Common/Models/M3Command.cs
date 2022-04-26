namespace Common.Models;

public record M3Command
{
    public string? CorrelationId { get; set; }

    public string? TenantId { get; set; }

    public string? EntityName { get; set; }

    public string? EventHandler { get; set; }

    public long FromSequenceNumber { get; set; }

    public object? Payload { get; set; }
}