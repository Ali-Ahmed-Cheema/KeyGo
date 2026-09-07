using KeyGo.Core.Models;
using KeyGo.Core.Services;
using Xunit;

namespace KeyGo.Core.Tests;

public sealed class CommandPolicyServiceTests
{
    [Fact]
    public void KnownVerificationCommandsAreSafe()
    {
        var result = new CommandPolicyService().Assess("dotnet test");

        Assert.Equal(CommandRisk.Safe, result.Risk);
    }

    [Fact]
    public void DependencyChangesRequireReview()
    {
        var result = new CommandPolicyService().Assess("npm install");

        Assert.Equal(CommandRisk.ReviewRequired, result.Risk);
    }

    [Fact]
    public void DestructiveCommandsAreBlocked()
    {
        var result = new CommandPolicyService().Assess("git reset --hard");

        Assert.Equal(CommandRisk.Blocked, result.Risk);
    }
}
