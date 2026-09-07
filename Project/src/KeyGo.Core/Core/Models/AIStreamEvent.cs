namespace KeyGo.Core.Models;

public sealed class AIStreamEvent
{
    public string Type { get; init; } = "text";
    public string Value { get; init; } = string.Empty;
    public bool IsFinal { get; init; }
}
