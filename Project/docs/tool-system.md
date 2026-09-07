# Tool System

`AgentToolRegistry` describes tools independently from their implementations. Each descriptor contains an ID, description, required permission, and risk level.

The default registry includes read-only project inspection tools plus explicitly high-risk modification and test tools. A tool implementation must check the permission service before performing a consequential operation.

The current file-change implementation is deliberately separate from the registry so proposals can be reviewed and diffed before execution.
