using KeyGo.Core.Models;

namespace KeyGo.Core.Services;

public sealed class CommandPolicyService
{
    private static readonly string[] SafeCommands = ["dotnet test", "dotnet build", "npm test", "npm run build", "pytest", "cargo test", "git status", "git diff", "git log"];
    private static readonly string[] ReviewCommands = ["npm install", "dotnet restore", "pip install", "cargo add"];
    private static readonly string[] BlockedFragments = ["format ", "format.com", "rm -rf", "rmdir /s", "del /s", "git push --force", "git reset --hard", "git clean -fd"];

    public CommandAssessment Assess(string command)
    {
        if (string.IsNullOrWhiteSpace(command)) return new CommandAssessment { Risk = CommandRisk.Blocked, Reason = "Command is empty." };
        var normalized = command.Trim().ToLowerInvariant();
        if (BlockedFragments.Any(fragment => normalized.Contains(fragment, StringComparison.Ordinal))) return new CommandAssessment { Command = command, Risk = CommandRisk.Blocked, Reason = "Destructive or irreversible command is blocked." };
        if (SafeCommands.Any(safe => normalized.Equals(safe, StringComparison.Ordinal))) return new CommandAssessment { Command = command, Risk = CommandRisk.Safe, Reason = "Known read-only or verification command." };
        if (ReviewCommands.Any(review => normalized.Equals(review, StringComparison.Ordinal))) return new CommandAssessment { Command = command, Risk = CommandRisk.ReviewRequired, Reason = "Command can change dependencies or the local environment." };
        return new CommandAssessment { Command = command, Risk = CommandRisk.ReviewRequired, Reason = "Unknown commands require explicit review." };
    }
}
