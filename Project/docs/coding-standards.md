# Coding Standards

## C# standards

- Use modern C# and nullable reference types
- Prefer explicit types and clear names over clever abstractions
- Keep classes single-responsibility and focused
- Use dependency injection for service creation
- Use async/await for all I/O-bound operations
- Pass `CancellationToken` through long-running work
- Dispose resources correctly
- Do not block on async work
- Avoid unnecessary allocations and global mutable state

## Architectural standards

- Keep UI logic out of the core runtime
- Keep provider-specific logic inside provider implementations
- Keep permissions and security decisions centralized
- Validate external input
- Never log secrets or raw credentials
- Keep public interfaces stable unless an ADR documents otherwise

## Design principles

- favor maintainability over cleverness
- prefer clear contracts over hidden behavior
- isolate platform-specific code behind abstraction boundaries
