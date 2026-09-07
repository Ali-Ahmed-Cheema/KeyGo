namespace KeyGo.Core.Models;

public enum AgentState
{
    Idle,
    Understanding,
    Investigating,
    Planning,
    WaitingForApproval,
    Executing,
    Testing,
    Verifying,
    Completed,
    Cancelled,
    Failed,
    BlockedByPermission,
    NeedsUserInput
}

public enum AgentMode
{
    Observe,
    Assist,
    Autonomous
}

public enum AgentPermission
{
    Read,
    Write,
    Delete,
    Execute,
    GitWrite,
    NetworkWrite
}

public enum ToolRiskLevel
{
    Low,
    Medium,
    High,
    Critical
}

public enum ApprovalState
{
    Pending,
    Approved,
    Rejected,
    Expired
}

public sealed class ToolDescriptor
{
    public string Id { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public AgentPermission RequiredPermission { get; init; }
    public ToolRiskLevel RiskLevel { get; init; }
}

public sealed class AgentPlan
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Goal { get; init; } = string.Empty;
    public IReadOnlyList<AgentPlanStep> Steps { get; init; } = Array.Empty<AgentPlanStep>();
    public IReadOnlyList<string> ToolsRequired { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> ExpectedFiles { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> Risks { get; init; } = Array.Empty<string>();
    public int EstimatedOperations { get; init; }
    public ApprovalState Approval { get; set; }
}

public sealed class AgentPlanStep
{
    public int Order { get; init; }
    public string Description { get; init; } = string.Empty;
    public string ToolId { get; init; } = string.Empty;
    public AgentPermission RequiredPermission { get; init; }
}

public sealed class AgentActivity
{
    public DateTime TimestampUtc { get; init; } = DateTime.UtcNow;
    public AgentState State { get; init; }
    public string Message { get; init; } = string.Empty;
}

public sealed class AgentSession
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string ProjectRoot { get; init; } = string.Empty;
    public string Goal { get; init; } = string.Empty;
    public AgentMode Mode { get; init; }
    public AgentState State { get; internal set; } = AgentState.Idle;
    public List<AgentActivity> Activity { get; } = new();
    public AgentPlan? Plan { get; internal set; }
}
