using KeyGo.Core.Abstractions;
using KeyGo.Core.Models;

namespace KeyGo.Core.Services;

public sealed class OpenAIProvider : IAIProvider
{
    private readonly ICredentialManager _credentialManager;
    private readonly HttpClient _httpClient;

    public OpenAIProvider(ICredentialManager credentialManager, HttpClient httpClient)
    {
        _credentialManager = credentialManager;
        _httpClient = httpClient;
    }

    public string Id => "openai";
    public string DisplayName => "OpenAI";

    public async Task<ProviderConnectionResult> ValidateAsync(CancellationToken cancellationToken = default)
    {
        var apiKey = await _credentialManager.GetAsync(Id, "default", cancellationToken);
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return ProviderConnectionResult.Failed(Id, "No API key configured.");
        }

        return ProviderConnectionResult.Connected(
            Id,
            "Credential configured and ready for verification.",
            new[] { "Authentication", "Model discovery", "Streaming" },
            new[] { "Vision", "Tool calling" });
    }

    public Task<IReadOnlyList<AIModel>> GetModelsAsync(CancellationToken cancellationToken = default)
    {
        var models = new List<AIModel>
        {
            new()
            {
                Id = "gpt-4o-mini",
                DisplayName = "GPT-4o Mini",
                Provider = Id,
                ContextLength = 128000,
                SupportsVision = true,
                SupportsAudio = false,
                SupportsTools = true,
                SupportsImageGeneration = false,
                SupportsReasoning = true,
                Status = "Verified"
            },
            new()
            {
                Id = "gpt-4o",
                DisplayName = "GPT-4o",
                Provider = Id,
                ContextLength = 128000,
                SupportsVision = true,
                SupportsAudio = true,
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
        var response = new AIResponse
        {
            Content = $"OpenAI provider received: {request.Prompt}",
            Provider = Id,
            Model = request.Model ?? "gpt-4o-mini",
            InputTokens = 120,
            OutputTokens = 80,
            EstimatedCost = 0.0012m,
            Status = "Estimated"
        };

        return Task.FromResult(response);
    }

    public async IAsyncEnumerable<AIStreamEvent> StreamAsync(AIRequest request, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var chunks = new[]
        {
            "OpenAI: ",
            "Analyzing your request...",
            "\n",
            request.Prompt
        };

        foreach (var chunk in chunks)
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return new AIStreamEvent { Type = "text", Value = chunk, IsFinal = false };
            await Task.Delay(50, cancellationToken);
        }

        yield return new AIStreamEvent { Type = "text", Value = string.Empty, IsFinal = true };
    }
}
