using KeyGo.Core.Models;
using KeyGo.Core.Services;
using Xunit;

namespace KeyGo.Core.Tests;

public sealed class AgentPermissionTests
{
    [Fact]
    public async Task ObserveMode_AllowsRead_ButRejectsWriteAndExecution()
    {
        var permissions = new AgentPermissionService(AgentMode.Observe);

        Assert.True(await permissions.CanExecuteAsync("ReadFile"));
        Assert.False(await permissions.CanExecuteAsync("ModifyFile"));
        Assert.False(await permissions.CanExecuteAsync("RunTests"));
    }

    [Fact]
    public async Task AssistMode_RequiresExplicitSessionGrantForWrites()
    {
        var permissions = new AgentPermissionService(AgentMode.Assist);

        Assert.False(await permissions.CanExecuteAsync("ModifyFile"));
        permissions.GrantForSession(AgentPermission.Write);
        Assert.True(await permissions.CanExecuteAsync("ModifyFile"));
    }
}
