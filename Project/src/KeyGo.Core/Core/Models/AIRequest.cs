namespace KeyGo.Core.Models;

public sealed class AIRequest
{
    public required string Provider { get; init; }
    public string? Model { get; init; }
    public string Prompt { get; init; } = string.Empty;
    public IReadOnlyDictionary<string, object>? Metadata { get; init; }
}
