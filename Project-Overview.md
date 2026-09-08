# KeyGo Project Specification

## Product Vision

KeyGo is a local-first AI development environment that connects cloud and open-source AI models to a developer's projects, tools, and computer. Developers can bring their own API keys, verify provider and model capabilities, run local models, intelligently route tasks across models, create AI agents, automate software work, test and repair applications, monitor usage and cost, personalize AI behavior, and control computer actions through a secure permission system.

Central philosophy:

KeyGo — One Developer. Any AI. Full Control.

---

## Problem Statement

On September 3, 2026, GPT-6 Astra was launched. As a software engineer, I immediately wanted to use the latest and most advanced AI model, but it was too expensive. I then discovered the broader issue behind the product idea: many developers want to bring their own API keys and use AI models directly, but they struggle with verification, security, local-first usage, provider compatibility, and project-aware workflows.

Many websites claim to offer "free" AI access by sharing or vending API keys, but the real problem is much deeper:

- Is the key valid?
- Is it actually for the model I expect?
- Is the provider real and compatible?
- Is the model's capability set what the app claims?
- Is the project context being sent safely and minimally?
- Can the user maintain control of credentials and tool actions?

KeyGo is designed to solve that problem for developers with a secure, local-first AI workspace.

---

## Product Pillars

KeyGo is best understood as five connected products in one:

| Pillar | Purpose |
|---|---|
| AI Gateway | API keys, providers, models, verification, health monitoring |
| AI Runtime | Routing, multimodal AI, personalization, memory, cost tracking |
| Developer Agent | Code, terminal, Git, project workspace, testing, diagnostics |
| Computer Agent | Controlled interaction with the user's PC and browser |
| AI Marketplace | Providers, models, agents, workflows, tools, extensions |

---

## Core Goals

1. Let a developer connect their own AI provider keys without exposing them to KeyGo servers.
2. Verify that a provider endpoint and model match the expected capabilities.
3. Keep the app local-first with credentials, history, and sensitive data under user control.
4. Provide project-aware AI workflows for coding, testing, repair, and analysis.
5. Route work to the best model based on cost, latency, privacy, and capability.
6. Let agents operate with explicit permissions and reversible checkpoints.
7. Make AI actions transparent, reviewable, and user-approved when sensitive actions are involved.

---

## Non-Goals for v1

KeyGo should not try to build the full long-range vision in one release. The v1 scope should stay focused on a workable foundation:

- Local runtime
- Provider adapters
- Secure key vault
- Chat experience
- Project workspace
- Agent and permission engine
- Basic routing and health checks

Later phases can add marketplace, multi-agent orchestration, local model management, browser automation, and autonomous computer control.

---

## User Experience

### Primary Experience: Mission Mode

Mission Mode should be the top-level experience.

Instead of asking users to choose an agent manually, KeyGo asks:

> Create Mission
>
> What do you want to accomplish?

Examples:

- Build a SaaS application from this specification.
- Fix issues in this repository.
- Explain this project architecture.
- Run a security audit and produce a patch plan.

KeyGo then decides:

- required capabilities
- required agents
- required tools
- best model or models
- permission requirements
- execution plan

A mission screen then shows progress like this:

Mission Control
BUILD SAAS APPLICATION

â–ˆâ–ˆâ–ˆâ–ˆâ–ˆâ–ˆâ–ˆâ–ˆâ–ˆâ–ˆâ–ˆâ–ˆâ–‘â–‘â–‘â–‘ 76%

âœ“ Requirements
âœ“ Architecture
âœ“ Database
âœ“ Backend
âœ“ Frontend
âœ“ Authentication
âœ“ Unit tests
âš™ Integration tests
â—‹ Security audit
â—‹ Documentation

---

## Core Features

### 1. AI Gateway

The app must support multiple providers in a normalized way:

- OpenAI
- Anthropic
- Google Gemini
- xAI
- Mistral
- OpenRouter
- Custom OpenAI-compatible providers

The gateway should support:

- API key management
- provider verification
- model list retrieval
- capability testing
- compatibility scoring
- token/cost estimation
- usage tracking

### 2. Capability Verification Engine

This is one of the most important features. KeyGo should not simply ask an AI, "Are you GPT-X?" It should independently verify the connection.

Flow:

API Key
â†“
Provider endpoint
â†“
Authentication test
â†“
Model metadata
â†“
Capability test
â†“
Response metadata
â†“
KeyGo Verification

Verification result example:

- API key authenticated
- Provider: OpenAI
- Requested model: gpt-4.1-mini
- Actual endpoint: api.openai.com
- Model available: Yes
- Text generation: Yes
- Vision: Yes
- Tool calling: Yes

Important note: KeyGo should not claim to cryptographically prove the underlying model identity when the provider does not expose authoritative metadata.

### 3. Provider Compatibility Score

For each provider or model family, KeyGo should show a compatibility score that indicates how well the provider supports the required features.

Example:

OpenAI compatibility
- Authentication: âœ“
- Streaming: âœ“
- Vision: âœ“
- Tools: âœ“
- Structured output: âœ“
- Token counting: âœ“
- Image generation: âœ“
- Embeddings: âœ“

Compatibility: 96%

Custom provider compatibility
- Authentication: âœ“
- Streaming: âœ“
- Vision: ?
- Tools: âœ—
- Token counting: ?
- Structured output: âœ“

Compatibility: 71%

This gives developers a clear indication of whether a provider is a good fit for their use case.

### 4. API Key Health

A key should not only be "valid" or "invalid". The app should show health states:

- Green: Healthy
- Yellow: Rate limited
- Orange: Low quota
- Red: Invalid
- Gray: Revoked

Also show:

- last successful request
- rate-limit information
- available models
- permissions
- provider
- usage
- estimated spending

This becomes a proper API control center.

### 5. AI Runtime

The runtime layer handles:

- prompt routing
- model selection
- multimodal requests
- personalization
- memory
- token/cost control
- project context assembly
- local vs cloud decision-making

The runtime must work with both cloud and local models.

---

## Security Architecture

### Local-First Principle

KeyGo should be designed around local-first security.

Local-first architecture:

KeyGo UI
â†“
Local Runtime
â†“
Local Project
Local History
Local Keys
â†“
AI Provider / Local Model

Key security rules:

- Server never receives the user's API key.
- The app connects directly to providers with the user's credentials.
- Stored credentials should use Windows secure storage or equivalent OS-level protections.
- Plaintext keys should never be stored in config files.
- Sensitive files should be excluded from cloud context automatically.

### Permission Firewall

KeyGo needs a dedicated permission firewall.

Files
- Read: âœ“
- Create: âœ“
- Modify: âœ“
- Delete: âš 

Terminal
- Safe commands: âœ“
- Package installs: âš 
- System commands: âœ—

Network
- AI providers: âœ“
- GitHub: âš 
- Unknown websites: âœ—

Credentials
- Read raw secrets: âœ—
- Export secrets: âœ—

This is a major trust feature for the product.

### AI Data Boundary

Before data leaves the device, KeyGo should display a clear boundary screen:

DATA LEAVING DEVICE

Model: Cloud Model X

Sending:
âœ“ User prompt
âœ“ 4 source files
âœ“ Error log

Not sending:
âœ“ .env
âœ“ API keys
âœ“ SSH keys
âœ“ passwords
âœ“ unrelated files

Then:
Approve & Send

### Secret Detection

Before sending project context to a cloud model:

âš  SENSITIVE DATA DETECTED

.env
AWS credentials
GitHub token
database password

KeyGo should exclude these files by default.

---

## AI Routing and Model Selection

KeyGo should choose the most appropriate model using a routing engine.

Routing priorities:

- task complexity
- context size
- privacy requirements
- hardware capability
- available models
- latency
- cost

Examples:

- "This is a simple question." â†’ Local model
- "Analyze my entire repository." â†’ Cloud model
- "I have sensitive source code." â†’ Local model
- "I need strong reasoning." â†’ Best available cloud model

### Local vs Cloud vs Hybrid

Cloud AI
- OpenAI
- Gemini
- Anthropic
- DeepSeek API
- Mistral
- xAI

Local AI
- DeepSeek
- Qwen
- Llama
- Mistral
- Gemma
- Phi

Hybrid AI
- Local analysis + cloud reasoning
- Local-first decision-making
- Cloud fallback when needed

### Privacy Mode

Private Mode

- Local model only
- No cloud API calls
- Project files remain on device
- Chat history remains local
- API keys unavailable to agents

---

## Developer Agent Experience

The developer agent should work directly with project files and repositories.

### Capabilities

- read project structure
- inspect relevant files
- understand architecture and dependencies
- propose changes
- create diffs
- run tests
- review output
- ask for approval before destructive actions

Example flow:

User: "Find why the login isn't working."

KeyGo:
â†’ scans project
â†’ identifies relevant files
â†’ analyzes code
â†’ proposes changes
â†’ shows diff
â†’ user approves
â†’ modifies files
â†’ runs tests
â†’ reports result

### Project Health Report

A project health report can include:

- code quality
- dependency health
- security warnings
- testing coverage
- documentation status
- overall score

Example:

PROJECT HEALTH
Overall: 82/100

âœ“ Git repository healthy
âœ“ Dependencies detected
âš  4 outdated packages
âš  2 security warnings
âš  7 TODOs
âš  3 large files
âœ“ Tests detected

### Built-in Cost and Usage Monitoring

KeyGo should track:

- input tokens
- output tokens
- total estimated tokens
- estimated cost
- model used
- provider used
- project context size
- session budgets

User budget controls:

- never exceed $5/day
- ask before a request estimated above $0.10
- custom thresholds

---

## Computer Agent

KeyGo should also be able to operate with the user's computer under controlled permissions.

### Permission Levels

Observe
- inspect files
- read project structure
- inspect processes
- read terminal output
- analyze screenshots
- cannot change anything

Assist
- create or edit project files
- run development commands
- install approved dependencies
- run tests
- open applications
- ask before sensitive actions

Autonomous
- run larger sequences of actions
- requires confirmation for:
  - deleting large numbers of files
  - changing system settings
  - accessing sensitive folders
  - external communications
  - purchases
  - credential changes
  - irreversible operations

### Browser Agent

KeyGo can launch a controlled browser session and:

- navigate websites
- test apps
- click buttons
- fill forms
- inspect pages
- capture screenshots
- identify UI issues
- test responsive layouts

### QA Agent

The system can create hundreds of test scenarios and discover edge cases automatically.

---

## Marketplace

The marketplace should include categories:

- Providers
- Models
- Agents
- Prompts
- Profiles
- Tools
- Extensions
- Themes
- Workflows

Workflow example:

React Bug Fix Workflow
- Analyze
- Reproduce
- Find cause
- Generate fix
- Run tests
- Review

---

## Personalization and Memory

Users should be able to define preferences that persist across models.

Examples:

- Always use TypeScript.
- Never modify tests without permission.
- Prefer functional React components.
- Explain complex changes before applying them.

This should be treated as personalization and feedback learning, not as a claim that the AI is being trained in a broad sense. Realistically the system changes instruction sets, preferences, routing rules, examples, and retrieval context.

---

## Checkpoint and Rollback

Before autonomous missions begin, KeyGo should create a checkpoint:

Checkpoint created

Project: my-store
Git commit: a81c92d
Files affected: 27

If the mission causes a bad change, the user should be able to roll back through:

- git commit / branch / worktree rollback when available
- reversible local history for non-Git projects

---

## Action Replay and Transparency

Every agent action should be reproducible.

MISSION #1842

â–¶ Step 1 â€” Read package.json
â–¶ Step 2 â€” Inspect src/
â–¶ Step 3 â€” Run npm test
â–¶ Step 4 â€” Analyze failure
â–¶ Step 5 â€” Modify auth.ts
â–¶ Step 6 â€” Run tests

The user can click each action and see why KeyGo made that decision.

---

## Offline Emergency Mode

If internet is unavailable, the app should switch to offline mode:

OFFLINE MODE

âœ“ Local models
âœ“ Local projects
âœ“ Local chat history
âœ“ Local agents
âœ“ Local tools

âœ— Cloud models
âœ— Cloud marketplace
âœ— Provider APIs

---

## VS Code Integration

KeyGo should include a VS Code extension that sits above the local runtime:

VS Code
â†•
KeyGo Extension
â†•
KeyGo Local Runtime
â†•
Model Router
â†•
AI Provider / Local Model

Features:

- Ask KeyGo about selected code
- Send a file to KeyGo
- Open KeyGo chat
- Apply a KeyGo-generated diff
- Run a KeyGo agent
- View token and cost data
- View project health
- Start and stop missions

The extension should not need to store the user's API keys.

---

## MVP Roadmap

### Phase 1 â€” Core Runtime

- local runtime
- secure key vault/storage
- provider adapters
- chat with project context
- project workspace integration
- permission engine
- token and usage tracking

### Phase 2 â€” Agentic Features

- project analysis
- code generation and edits
- git-aware workflows
- test execution
- diff approval flow

### Phase 3 â€” Model Intelligence

- routing engine
- compatibility checks
- local/cloud/hybrid decisions
- personalized memory and preferences

### Phase 4 â€” Computer Control

- file actions
- browser automation
- terminal automation
- permission firewall

### Phase 5 â€” Marketplace and Ecosystem

- model marketplace
- workflow packs
- agent templates
- community sharing

---

## Final Positioning

KeyGo is not just a BYOK chat application. It is a local-first AI development environment for developers who want to:

- use any AI model they choose
- keep control of their keys and data
- work directly on their projects
- automate engineering work with permission boundaries
- route work intelligently across providers and local models
- use AI as a development collaborator rather than a black box

That is the long-term product vision.

---

## Summary

The product is most compelling when it is positioned as a secure, local-first AI operating environment for software work. The right MVP is not the entire multi-year visionary roadmap. The right starting point is:

- Local Runtime
- Provider Adapters
- Secure Key Vault
- Chat + Project Workspace
- Agent/Permission Engine

From there, KeyGo can progressively add:

- marketplace
- local models
- browser and terminal automation
- multi-agent workflows
- advanced autonomous engineering features

This creates a realistic path from MVP to the full KeyGo vision.

