namespace KeyGo.Core.Models;

public sealed class UsageRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid? ConversationId { get; set; }
    public string Provider { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public long InputTokens { get; set; }
    public long OutputTokens { get; set; }
    public long CachedTokens { get; set; }
    public long ReasoningTokens { get; set; }
    public decimal EstimatedCost { get; set; }
    public string UsageStatus { get; set; } = "Estimated";
    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
    public string RequestStatus { get; set; } = "Completed";
    public int? LatencyMs { get; set; }
}
