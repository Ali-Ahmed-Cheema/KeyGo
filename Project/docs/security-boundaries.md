# Security Boundaries

## Implemented

- Local-first credential storage abstraction
- Project context secret scanning and exclusion for likely secrets
- Privacy-mode guard that rejects cloud context when local-only mode is enabled
- Audit-friendly request metadata at the runtime and data model layers

## Experimental

- Inline secret-redaction patterns and dismissive handling of likely credential files

## Planned

- Real Windows Credential Manager / DPAPI integration
- Full redaction and sanitization of logs, headers, and provider payloads
- Security tests covering log sanitization, secret exclusion, and permission enforcement
- Explicit boundary between UI, runtime, and cloud provider transport
