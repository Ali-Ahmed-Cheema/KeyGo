using KeyGo.Core.Abstractions;

namespace KeyGo.Core.Services;

public sealed class SimpleUsageTracker : IUsageTracker
{
    private long _inputTokens;
    private long _outputTokens;
    private decimal _estimatedCost;

    public Task TrackRequestAsync(string provider, string model, long inputTokens, long outputTokens, decimal estimatedCost, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        _inputTokens += inputTokens;
        _outputTokens += outputTokens;
        _estimatedCost += estimatedCost;

        return Task.CompletedTask;
    }

    public Task<UsageSummary> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var summary = new UsageSummary(_inputTokens, _outputTokens, _estimatedCost, "Estimated");
        return Task.FromResult(summary);
    }
}
