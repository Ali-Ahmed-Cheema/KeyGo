using KeyGo.Core.Abstractions;
using KeyGo.Core.Models;

namespace KeyGo.Core.Services;

public sealed class AnthropicProvider : IAIProvider
{
    private readonly ICredentialManager _credentialManager;

    public AnthropicProvider(ICredentialManager credentialManager)
    {
        _credentialManager = credentialManager;
    }

    public string Id => "anthropic";
    public string DisplayName => "Anthropic";

    public async Task<ProviderConnectionResult> ValidateAsync(CancellationToken cancellationToken = default)
    {
        var apiKey = await _credentialManager.GetAsync(Id, "default", cancellationToken);
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return ProviderConnectionResult.Failed(Id, "No Anthropic API key configured.");
        }

        return ProviderConnectionResult.Connected(
            Id,
            "Anthropic credential is configured.",
            new[] { "Authentication", "Model discovery", "Streaming" },
            new[] { "Vision", "Audio" });
    }

    public Task<IReadOnlyList<AIModel>> GetModelsAsync(CancellationToken cancellationToken = default)
    {
        var models = new List<AIModel>
        {
            new()
            {
                Id = "claude-3-5-sonnet",
                DisplayName = "Claude 3.5 Sonnet",
                Provider = Id,
                ContextLength = 200000,
                SupportsVision = true,
                SupportsAudio = false,
                SupportsTools = true,
                SupportsImageGeneration = false,
                SupportsReasoning = true,
                Status = "Verified"
            },
            new()
            {
                Id = "claude-3-opus",
                DisplayName = "Claude 3 Opus",
                Provider = Id,
                ContextLength = 200000,
                SupportsVision = true,
                SupportsAudio = false,
                SupportsTools = true,
                SupportsImageGeneration = false,
                SupportsReasoning = true,
                Status = "Verified"
            }
        };

        return Task.FromResult<IReadOnlyList<AIModel>>(models);
    }

    public Task<AIResponse> SendAsync(AIRequest request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new AIResponse
        {
            Content = $"Anthropic provider received: {request.Prompt}",
            Provider = Id,
            Model = request.Model ?? "claude-3-5-sonnet",
            InputTokens = 130,
            OutputTokens = 85,
            EstimatedCost = 0.0015m,
            Status = "Estimated"
        });
    }

    public async IAsyncEnumerable<AIStreamEvent> StreamAsync(AIRequest request, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var chunks = new[]
        {
            "Anthropic: ",
            "Processing your request...",
            "\n",
            request.Prompt
        };

        foreach (var chunk in chunks)
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return new AIStreamEvent { Type = "text", Value = chunk, IsFinal = false };
            await Task.Delay(35, cancellationToken);
        }

        yield return new AIStreamEvent { Type = "text", Value = string.Empty, IsFinal = true };
    }
}
