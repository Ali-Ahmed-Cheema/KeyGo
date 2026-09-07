# Coding Agent Architecture

KeyGo's coding agent is provider-neutral and approval-gated. The current foundation is:

```text
User goal
  -> CodingAgentOrchestrator
  -> ProjectWorkspaceService / ProjectContextService
  -> structured AgentPlan
  -> explicit approval
  -> FileChangeService
```

## State machine

Sessions expose `Idle`, `Understanding`, `Investigating`, `Planning`, `WaitingForApproval`, `Executing`, `Testing`, `Verifying`, `Completed`, `Cancelled`, `Failed`, `BlockedByPermission`, and `NeedsUserInput`.

Investigation completes without modifying files. Plans contain the goal, ordered steps, tools, expected files, risks, and estimated operations. A plan must be approved before execution can begin.

## Safety model

`AgentPermissionService` is the enforcement layer. Observe mode allows reads only. Assist mode still requires an explicit session grant for write, delete, or execution permissions. The UI must not be treated as the security boundary.

## Current scope

Implemented: stateful investigation, structured plans, tool descriptors, permission checks, proposed file changes, unified review output, approval-gated transactional writes, path validation, and audit hooks.

Not yet implemented: terminal execution, Git mutation, Roslyn symbols, persistent session recovery, UI, and provider-backed plan generation.
