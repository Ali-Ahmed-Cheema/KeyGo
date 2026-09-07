# Provider Architecture

## Goal

Support multiple AI providers without coupling the runtime to one vendor. KeyGo must support both standard clouds and custom OpenAI-compatible endpoints.

## Provider contract

The provider layer should expose common operations such as:

- send message
- stream response
- generate image
- analyze image
- generate embeddings
- count tokens
- estimate cost
- list models

## Initial supported providers

- OpenAI
- Google Gemini
- Anthropic
- DeepSeek
- Mistral
- xAI
- OpenRouter
- custom OpenAI-compatible APIs

## Abstraction model

```text
IAIProvider
  ├── OpenAIProvider
  ├── GoogleProvider
  ├── AnthropicProvider
  ├── DeepSeekProvider
  ├── MistralProvider
  ├── XAIProvider
  ├── OpenRouterProvider
  └── OpenAICompatibleProvider
```

## Capability system

Each model declares capabilities such as:

- TextGeneration
- Streaming
- Vision
- ImageGeneration
- AudioInput
- AudioOutput
- ToolCalling
- StructuredOutput
- Embeddings
- Reasoning
- LongContext
- CodeExecution

## Verification process

When a user adds a provider key, KeyGo should validate:

- authentication
- endpoint accessibility
- model availability
- reported capabilities
- streaming support
- compatibility score for unknown providers

## Provider compatibility reporting

Compatibility tests should distinguish:

- requested model
- provider-reported model
- endpoint
- observed capabilities
- independently verified results

This keeps model identity and capability claims explicit and honest.
