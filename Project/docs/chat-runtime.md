# Chat Runtime

## Implemented

- `WorkspaceChatService` composes project context and chat input
- Request building with prompt and metadata
- Streaming response consumption from provider adapters
- Conversation persistence from user and assistant messages
- Context attachment to conversation creation and retrieval

## Experimental

- Running chat from the desktop shell with model selection and project-root context
- Basic response text streaming plus estimated token accounting

## Planned

- Real message list UI, markdown rendering, code block presentation, copy actions, and retry flows
- Streaming state, cancellation, and timeout handling in the UI layer
- Conversation switching, provider/model state persistence, and usage tracking tied to each request
