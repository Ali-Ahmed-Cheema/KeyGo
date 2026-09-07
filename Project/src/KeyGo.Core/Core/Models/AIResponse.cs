namespace KeyGo.Core.Models;

public sealed class AIResponse
{
    public required string Content { get; init; } = string.Empty;
    public required string Provider { get; init; }
    public string? Model { get; init; }
    public long InputTokens { get; init; }
    public long OutputTokens { get; init; }
    public decimal EstimatedCost { get; init; }
    public string Status { get; init; } = "Unknown";
}
