namespace Common.Rejections
{
    public record FailedArea
    {
        public string? Area { get; init; }
        public int? FailedCount { get; set; }
    }
}
