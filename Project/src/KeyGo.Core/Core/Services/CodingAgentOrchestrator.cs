using KeyGo.Core.Abstractions;
using KeyGo.Core.Models;

namespace KeyGo.Core.Services;

public sealed class CodingAgentOrchestrator
{
    private readonly ProjectWorkspaceService _workspace;
    private readonly ProjectContextService _context;
    private readonly IAuditLogger? _auditLogger;

    public CodingAgentOrchestrator(ProjectWorkspaceService workspace, ProjectContextService context, IAuditLogger? auditLogger = null)
    {
        _workspace = workspace;
        _context = context;
        _auditLogger = auditLogger;
    }

    public async Task<AgentSession> InvestigateAsync(string projectRoot, string goal, AgentMode mode = AgentMode.Observe, CancellationToken cancellationToken = default)
    {
        var root = await _workspace.OpenProjectAsync(projectRoot, cancellationToken);
        var session = new AgentSession { ProjectRoot = root, Goal = goal, Mode = mode };
        Transition(session, AgentState.Understanding, "Understanding project");
        await _workspace.IndexAsync(root, cancellationToken);
        Transition(session, AgentState.Investigating, "Investigating relevant project files");
        await _context.BuildAsync(root, goal, cloudRequest: false, cancellationToken);
        Transition(session, AgentState.Completed, "Investigation completed; no files were modified");
        return session;
    }

    public AgentPlan CreatePlan(AgentSession session, IReadOnlyList<AgentPlanStep> steps, IReadOnlyList<string>? expectedFiles = null, IReadOnlyList<string>? risks = null)
    {
        if (session.State != AgentState.Investigating && session.State != AgentState.Completed) throw new InvalidOperationException("A plan can only be created after investigation.");
        var plan = new AgentPlan
        {
            Goal = session.Goal,
            Steps = steps,
            ToolsRequired = steps.Select(step => step.ToolId).Distinct(StringComparer.OrdinalIgnoreCase).ToArray(),
            ExpectedFiles = expectedFiles ?? Array.Empty<string>(),
            Risks = risks ?? Array.Empty<string>(),
            EstimatedOperations = steps.Count,
            Approval = ApprovalState.Pending
        };
        session.Plan = plan;
        Transition(session, AgentState.WaitingForApproval, "Plan generated; waiting for user approval");
        return plan;
    }

    public void ApprovePlan(AgentSession session)
    {
        if (session.Plan is null || session.State != AgentState.WaitingForApproval) throw new InvalidOperationException("No pending plan is available for approval.");
        session.Plan.Approval = ApprovalState.Approved;
        Transition(session, AgentState.Executing, "Plan approved; execution is now permitted");
    }

    public void RejectPlan(AgentSession session)
    {
        if (session.Plan is null || session.State != AgentState.WaitingForApproval) throw new InvalidOperationException("No pending plan is available for rejection.");
        session.Plan.Approval = ApprovalState.Rejected;
        Transition(session, AgentState.Cancelled, "Plan rejected by user");
    }

    private void Transition(AgentSession session, AgentState state, string message)
    {
        session.State = state;
        session.Activity.Add(new AgentActivity { State = state, Message = message });
        _ = _auditLogger?.LogAsync($"Agent {session.Id}: {state} - {message}");
    }
}
