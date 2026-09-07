namespace KeyGo.Core.Abstractions;

public interface IUsageTracker
{
    Task TrackRequestAsync(string provider, string model, long inputTokens, long outputTokens, decimal estimatedCost, CancellationToken cancellationToken = default);
    Task<UsageSummary> GetSummaryAsync(CancellationToken cancellationToken = default);
}

public sealed record UsageSummary(
    long InputTokens,
    long OutputTokens,
    decimal EstimatedCost,
    string Status);
