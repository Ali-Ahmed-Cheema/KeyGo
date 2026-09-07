# Testing Strategy

## Goals

- validate runtime correctness
- verify provider behavior and compatibility
- protect security-sensitive paths
- keep regression issues visible

## Test pyramid

### Unit tests
Focus on isolated logic and deterministic rules.

Examples:
- model routing logic
- usage summaries
- permission decisions
- data minimization checks

### Integration tests
Test runtime interactions with SQLite and provider adapters using controlled scenarios.

### Security tests
Validate:

- raw API keys are not logged
- secrets are not exposed in audit output
- permission checks block disallowed actions
- local-only rules are enforced

### Provider tests
Focus on:

- endpoint validation
- capability discovery
- compatibility reporting
- streaming behavior where supported

### Regression tests
Add tests for previous bugs and broken assumptions before shipping changes.

## Tooling

- xUnit
- FluentAssertions optional if needed later
- test fixtures for chat sessions, projects, and provider metadata
