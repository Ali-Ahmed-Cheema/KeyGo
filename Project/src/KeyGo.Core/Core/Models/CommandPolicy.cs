namespace KeyGo.Core.Models;

public enum CommandRisk
{
    Safe,
    ReviewRequired,
    Dangerous,
    Blocked
}

public sealed class CommandAssessment
{
    public string Command { get; init; } = string.Empty;
    public CommandRisk Risk { get; init; }
    public string Reason { get; init; } = string.Empty;
}
