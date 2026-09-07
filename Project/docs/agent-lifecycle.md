# Agent Lifecycle

A coding-agent session exposes observable state transitions:

`Idle -> Understanding -> Investigating -> Planning -> WaitingForApproval -> Executing -> Testing -> Verifying -> Completed`

Failure and interruption states are explicit: `Cancelled`, `Failed`, `BlockedByPermission`, and `NeedsUserInput`.

The current orchestrator implements project opening, indexing, investigation, structured plan creation, plan approval, plan rejection, and activity entries. It does not silently move from investigation to file modification.
