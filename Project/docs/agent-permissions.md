# Agent Permissions

Default agent mode is `Observe`.

| Capability | Observe | Assist |
| --- | --- | --- |
| Read/search/analyze | Allowed | Allowed |
| Modify/create files | Denied | Requires session grant |
| Delete files | Denied | Requires session grant |
| Run tests/commands | Denied | Requires session grant |
| Git commit/push | Denied | Not exposed by default |

Permission checks are performed in `AgentPermissionService`, not only in UI controls. File operations also require an approved proposal and a project-root boundary check.
