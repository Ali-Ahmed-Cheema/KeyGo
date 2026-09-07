namespace KeyGo.Core.Models;

public sealed class AIModel
{
    public required string Id { get; init; }
    public string? DisplayName { get; init; }
    public string? Provider { get; init; }
    public int? ContextLength { get; init; }
    public bool? SupportsVision { get; init; }
    public bool? SupportsAudio { get; init; }
    public bool? SupportsTools { get; init; }
    public bool? SupportsImageGeneration { get; init; }
    public bool? SupportsReasoning { get; init; }
    public decimal? InputPricePer1M { get; init; }
    public decimal? OutputPricePer1M { get; init; }
    public string Status { get; init; } = "Unknown";
}
