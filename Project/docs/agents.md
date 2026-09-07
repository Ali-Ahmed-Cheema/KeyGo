# Agent Architecture

## Design goals

KeyGo must avoid giving AI unrestricted computer access. The runtime should mediate all actions through permission checks and tool execution boundaries.

## Model-to-tool flow

```text
AI Model
   ↓
Agent
   ↓
Tool Request
   ↓
Permission Service
   ↓
Policy Engine
   ↓
Tool Executor
   ↓
Operating System
```

## Tool abstraction

Each tool must declare:

- name
- description
- parameters
- required permission
- risk level
- reversibility
- supported environments

Potential tools:

- FileTool
- SearchTool
- TerminalTool
- GitTool
- ProjectTool
- BrowserTool
- VSCodeTool
- ComputerTool

## Permissions

Support permission levels such as:

- Observe
- Assist
- Autonomous

Sensitive operations must require explicit approval, including:

- deleting files
- installing software
- changing system settings
- working with credentials
- sending external communication
- purchases or deployment actions

## Audit requirements

Every consequential action should be traceable in the audit log, including:

- reads
- writes
- test execution
- fails and retries
- user approvals
- final outcomes

The system must never log secrets.
