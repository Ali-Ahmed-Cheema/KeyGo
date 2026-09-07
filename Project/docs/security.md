# KeyGo Security

## Security goals

KeyGo is designed around a local-first and privacy-first model. The user's API keys, project context, and sensitive files must remain under the user's control unless the user explicitly authorizes remote use.

## Critical rules

- Never send raw API keys to KeyGo servers
- Store credentials in OS-protected storage when possible
- Keep data on the local machine unless user opts in to cloud transmission
- Enforce explicit permissions before tool execution
- Never give the AI unrestricted filesystem or terminal access
- Log audit events without storing secrets

## Credential storage

The credential system should use:

- Windows Credential Manager
- DPAPI-backed storage where appropriate
- database references instead of raw key values

Recommended schema:

```text
CredentialRecord
- Id
- Provider
- Name
- CredentialKeyRef
- CreatedAt
- UpdatedAt
- IsActive
```

## Threat model summary

### Primary risks

- API key leakage
- exposure of local project files
- uncontrolled file modifications
- unsafe terminal execution
- unsafe external network calls
- excessive token costs
- unauthorized model capability claims

### Countermeasures

- secure local credential store
- permission boundaries
- explicit user approval for sensitive actions
- review of data sent to cloud providers
- audit logging
- rate limiting and budget checks
- provider compatibility verification before trusting metadata

## Data minimization

Before transmitting to a cloud provider, KeyGo should determine what data is leaving the machine. Sensitive files such as `.env`, private keys, credentials, and secrets should be excluded by default.

## Privacy mode

Provide a privacy mode that enforces:

- local models only
- no cloud AI requests
- local project and chat history
- local credentials only

## Audit logging

Every consequential action should be recorded without exposing secrets. The audit log should capture file reads, modifications, tests, approvals, token events, and model selection decisions.
