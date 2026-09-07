# ADR 0001: Use a modular core runtime with provider abstractions

## Status
Accepted

## Context
KeyGo needs to support many AI providers, different model capabilities, local-first storage, and controlled tool execution. A monolithic app would make provider changes, safety constraints, and long-term maintenance harder.

## Decision
Create a modular runtime with explicit abstractions for providers, models, projects, permissions, usage, and audit logging.

## Consequences

### Positive
- easier provider support
- better security boundaries
- cleaner testability
- clearer architecture for future features

### Negative
- more upfront design work
- interfaces must be kept stable
- additional maintenance overhead for module boundaries
