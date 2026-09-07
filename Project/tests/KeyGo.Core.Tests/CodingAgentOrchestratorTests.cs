using KeyGo.Core.Models;
using KeyGo.Core.Services;
using Xunit;

namespace KeyGo.Core.Tests;

public sealed class CodingAgentOrchestratorTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "KeyGoAgentTests", Guid.NewGuid().ToString("N"));

    public CodingAgentOrchestratorTests()
    {
        Directory.CreateDirectory(Path.Combine(_root, "src"));
        File.WriteAllText(Path.Combine(_root, "src", "AuthService.cs"), "class AuthService { void Login() { } }");
    }

    [Fact]
    public async Task InvestigationThenPlanRequiresApprovalBeforeExecution()
    {
        var workspace = new ProjectWorkspaceService();
        var context = new ProjectContextService(workspace);
        var orchestrator = new CodingAgentOrchestrator(workspace, context);

        var session = await orchestrator.InvestigateAsync(_root, "Find AuthService");
        var plan = orchestrator.CreatePlan(session, new[]
        {
            new AgentPlanStep { Order = 1, Description = "Inspect source", ToolId = "ReadFile", RequiredPermission = AgentPermission.Read },
            new AgentPlanStep { Order = 2, Description = "Propose a change", ToolId = "ModifyFile", RequiredPermission = AgentPermission.Write }
        });

        Assert.Equal(AgentState.WaitingForApproval, session.State);
        Assert.Equal(ApprovalState.Pending, plan.Approval);
        orchestrator.ApprovePlan(session);
        Assert.Equal(AgentState.Executing, session.State);
    }

    [Fact]
    public async Task RejectPlanCancelsSession()
    {
        var workspace = new ProjectWorkspaceService();
        var context = new ProjectContextService(workspace);
        var orchestrator = new CodingAgentOrchestrator(workspace, context);
        var session = await orchestrator.InvestigateAsync(_root, "Inspect project");
        orchestrator.CreatePlan(session, Array.Empty<AgentPlanStep>());

        orchestrator.RejectPlan(session);

        Assert.Equal(AgentState.Cancelled, session.State);
        Assert.Equal(ApprovalState.Rejected, session.Plan!.Approval);
    }

    public void Dispose()
    {
        if (Directory.Exists(_root)) Directory.Delete(_root, recursive: true);
    }
}
