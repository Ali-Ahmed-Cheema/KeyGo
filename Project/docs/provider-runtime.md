# Provider Runtime

## Implemented

- Common provider contract via `IAIProvider`
- Production-like `OpenAIProvider`, `GeminiProvider`, and `AnthropicProvider`
- Provider verification and model-list retrieval
- Normalized capability reporting with explicit unsupported capability lists
- Request/response contracts for provider-neutral runtime usage

## Experimental

- Live cost calculation based on provider pricing metadata
- Tiered capability negotiation for vision, tool calling, audio, and embeddings
- Provider-specific behavior beyond text generation and streaming

## Planned

- OpenAI-compatible custom endpoints
- DeepSeek, xAI, Mistral, OpenRouter, and local provider adapters
- Stronger error mapping and retry policies with cancellation and timeout handling
- Centralized provider registry and connection-state persistence
