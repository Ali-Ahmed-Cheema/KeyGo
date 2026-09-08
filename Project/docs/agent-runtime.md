# Agent Runtime

## Implemented

- Agent states and lifecycle transitions
- Planning primitives with expected files and risk notes
- Permission-grant model for observe and higher-risk operations
- Approval-based plan flow
- File change proposal and diff-oriented workflow primitives

## Experimental

- Basic agent orchestration for investigation and context gathering
- Tool registry for read, search, and change operations

## Planned

- Real tool adapters for file edits, terminal execution, test commands, and Git actions
- Human approval for patches and hunk-level changes
- Permission enforcement integrated into all tool execution paths
- Full agent loop with execution, verification, and reporting
