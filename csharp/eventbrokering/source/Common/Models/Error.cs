namespace Common.Models
{
    public record Error
    {
        public string? Message { get; set; }
        public string? Code { get; set; }
    }
}
