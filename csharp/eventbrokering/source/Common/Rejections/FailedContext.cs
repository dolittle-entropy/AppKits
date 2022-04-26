namespace Common.Rejections
{
    public record FailedContext
    {
        public string? Context { get; init; }
        public int? FailedCount { get; init; }
    }
}
