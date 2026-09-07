# KeyGo

KeyGo is a local-first AI development environment for Windows. It enables developers to connect their own API keys, route work across multiple providers, work against local project folders, track usage and costs, and safely operate within permission boundaries.

## Project status

Current milestone: KeyGo Core Alpha

## Goals

- Keep user credentials local and secure
- Support multiple AI providers through a common abstraction
- Use project context without exposing sensitive files
- Track token and cost usage with estimates and budgets
- Allow permission-gated tool execution for coding agents
- Build a modular architecture that can evolve over time

## Solution structure

- src/KeyGo.Core — core runtime, abstractions, provider model, cost logic, project intelligence
- src/KeyGo.App — desktop shell and application bootstrap
- tests/KeyGo.Core.Tests — unit and integration tests
- docs/ — architecture and design documentation
- adr/ — architecture decision records

## Technology baseline

- C#
- .NET 10 LTS
- SQLite
- EF Core
- Serilog
- WinUI 3
- xUnit

## Quick start

1. Install .NET 10 SDK.
2. Open the solution in Visual Studio or VS Code.
3. Restore packages.
4. Run the App project.

## Security note

API keys must never be stored in plaintext or sent to KeyGo servers. The design assumes local-only credential storage using Windows secure storage facilities.

## See also

- docs/architecture.md
- docs/security.md
- docs/providers.md
- docs/agents.md
- docs/permissions.md
- docs/contributing.md
