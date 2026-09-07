# Permissions and Safety Model

## Permission principles

The user remains the final authority for sensitive and reversible actions. AI-generated suggestions are not equivalent to permitted execution.

## Permission levels

- Observe: read and analyze only
- Assist: read plus controlled modifications
- Autonomous: permitted with explicit boundaries and approval gates

## Examples of sensitive actions

- modifying project files without review
- deleting directories
- editing credentials
- changing system settings
- installing packages or software
- production deployment
- external network communication
- purchasing products/services

## Policy enforcement

KeyGo should centralize all permission decisions behind a policy service instead of allowing tools to execute directly.

## Guardrails

- require user confirmation before risky actions
- use checkpoints and rollback when possible
- support Git-based checkpoints for project edits
- always show diff before applying changes
- run validation after modifications
