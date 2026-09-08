# Current Implementation Status

## Scope

This repository is now a working core alpha for a local-first AI project workspace. The implementation is no longer just a collection of interfaces; it contains an actual runtime path for provider validation, project indexing, context building, chat streaming, and persistence.

## Implemented

- Provider abstractions and production-style provider adapters for OpenAI, Gemini, and Anthropic
- Normalized provider capability reporting via `ProviderCapability` and `ProviderCapabilities`
- Secure local credential storage abstraction with in-process credential manager behavior
- Project workspace indexing with file classification, filtering, safe relative reads, and search
- Project context assembly with token estimation and secret detection exclusions
- Streaming chat service that assembles project context, sends a request, and streams response text
- EF Core chat persistence for conversations and messages
- Usage tracking abstraction and basic usage summary model
- Agent orchestration states, plan creation, and approval workflow primitives
- Core tests covering provider validation, capability detection, model discovery, and chat persistence

## Experimental

- WinUI app shell composition and view-model bootstrapping
- Provider verification and model discovery flow against live endpoints
- Model-routing heuristics for task-to-model selection
- Cross-provider capability comparisons and cost estimation heuristics

## Planned

- Real Windows Credential Manager / DPAPI-backed secure storage
- Full provider connection center with UI state and connection lifecycle management
- Persistent usage, audit, provider-connection, and model cache tables beyond the current minimal schema
- Budget enforcement and per-request confirmation controls
- Permission-gated file editing, diff approval, and test execution pipeline
- Command palette, privacy/offline mode, smart routing, and background indexing
- Full WinUI chat workspace, project explorer, context inspector, and activity timeline

## Current Runtime Map

```text
KeyGo.App
  └── AppHost
        ├── ProjectWorkspaceService
        ├── ProviderManager
        │     ├── OpenAIProvider
        │     ├── GeminiProvider
        │     └── AnthropicProvider
        ├── ProjectContextService
        ├── WorkspaceChatService
        ├── ConversationService
        └── CodingAgentOrchestrator
```

## Status

The project is in a solid alpha-ready state for the provider + project-context + streaming slice, but it still requires additional product work before reaching the full milestone described in the roadmap.
