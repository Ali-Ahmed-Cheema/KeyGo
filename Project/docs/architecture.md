# KeyGo Architecture

## Objective

KeyGo is designed as a secure, local-first AI development environment. The runtime orchestrates AI requests, project understanding, tool execution, and permissions without giving the model unrestricted control over the machine.

## High-level architecture

```text
                         KEYGO
                           │
                    ┌──────┴──────┐
                    │   KeyGo UI  │
                    └──────┬──────┘
                           │
                    Application Layer
                           │
                    KeyGo Runtime
                           │
       ┌───────────────────┼───────────────────┐
       ↓                   ↓                   ↓
 Model Router         Agent Engine       Project Engine
       │                   │                   │
       └───────────────────┼───────────────────┘
                           ↓
                    Core Abstractions
                           │
        ┌──────────────────┼───────────────────┐
        ↓                  ↓                   ↓
 Provider System     Security System      Storage System
        │                  │                   │
        ↓                  ↓                   ↓
 OpenAI/Google/etc.   Credentials       SQLite
                      Permissions
                      Policies
                           │
                           ↓
                     Tool System
                           │
              ┌────────────┼────────────┐
              ↓            ↓            ↓
           Files        Terminal       Git
              │            │            │
              └────────────┼────────────┘
                           ↓
                      User Computer
```

## Architectural principles

- Local-first
- Privacy-first
- Security-first
- Provider-neutral
- Model-neutral
- Capability-aware
- Human-controlled
- Observability-first

## Core runtime layers

### 1. UI layer
The UI should be a thin shell that presents chat, project context, routing choices, permissions, and status. It does not contain provider-specific logic.

### 2. Application layer
The application layer handles orchestration, app lifecycle, and service composition. It is responsible for bootstrapping a runtime session and validating user settings.

### 3. Runtime layer
This contains the major orchestration services:

- model routing
- conversation orchestration
- project context selection
- permission enforcement
- usage/cost tracking
- tool invocation
- audit logging

### 4. Provider abstraction layer
Providers implement a stable contract so the app can support OpenAI-compatible endpoints, Google, Anthropic, Mistral, OpenRouter, local models, and custom providers without rewriting the core.

### 5. Tool and agent layer
The agent layer decides what to do; the tool layer performs permitted work; the permission layer decides whether the action is allowed.

## Phase 1 scope

The first implementation milestone is KeyGo Core Alpha and includes:

- secure API key registration
- provider verification
- chat with streaming
- model selection
- local project folder access
- SQLite-backed chat history
- basic token/cost tracking
- permission model for tools and actions

## Non-goals for the initial phase

- marketplace
- multi-agent factory
- computer automation
- full autonomous mission orchestration
- large local model manager

## Design guidance

Prefer maintainability over cleverness. The runtime should be modular, interface-driven, and testable. Keep provider-specific logic isolated and keep security decisions out of the AI prompt pipeline.

## Project intelligence foundation

The current project engine keeps the original repository in place and stores an in-memory index for the active process. `ProjectWorkspaceService` owns root normalization, ignore rules, file classification, evidence-based detection, indexing, search, and safe relative reads. `ProjectContextService` consumes those results and applies relevance ranking, secret filtering, privacy-mode enforcement, and a bounded context budget before any provider request.

This boundary is deliberately read-only. File writes, deletes, terminal execution, Git mutation, and deployment are not part of the project tool surface.
