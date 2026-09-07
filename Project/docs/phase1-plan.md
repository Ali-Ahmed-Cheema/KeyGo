# Phase 1 — KeyGo Core Alpha

## Objective

Build the minimum stable runtime that allows a user to install KeyGo, securely add an API key, verify a provider, choose a model, chat with it, open a local project, and use project context without sending credentials to KeyGo servers.

## In scope

- Windows desktop app shell
- provider abstraction
- secure credential storage
- model verification flow
- chat and streaming support
- SQLite-backed chat history
- project folder access
- token and cost tracking
- basic permission model

## Out of scope

- marketplace
- multi-agent software factory
- computer agent
- large local model manager

## Delivery milestones

### Milestone 1: Runtime foundation
- dependency injection
- logging
- configuration
- database bootstrap
- error handling

### Milestone 2: Provider system
- common provider interface
- initial provider adapters
- verification and capability detection

### Milestone 3: Chat and model layer
- chat session model
- message storage
- model selection
- streaming responses

### Milestone 4: Project awareness
- project discovery
- file indexing
- context selection
- data minimization checks

### Milestone 5: Security and permissions
- secure credentials
- policies
- audit log
- budget checks

### Milestone 6: UX and validation
- review screens
- provider verification report
- app-level tests
