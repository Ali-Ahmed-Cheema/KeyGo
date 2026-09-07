# Terminal Security

The coding agent does not execute commands by default. `CommandPolicyService` is the classification boundary for a future approved execution tool.

- Known verification commands such as `dotnet test`, `dotnet build`, `npm test`, and `git status` are classified as `Safe`.
- Dependency/environment changes such as `npm install` require review.
- Destructive operations such as force push, hard reset, recursive deletion, and disk formatting are blocked.
- Unknown commands require review.

Classification is not authorization. A future execution tool must still require the `Execute` permission, explicit command approval, a bounded working directory, cancellation, timeout, and audit logging.
