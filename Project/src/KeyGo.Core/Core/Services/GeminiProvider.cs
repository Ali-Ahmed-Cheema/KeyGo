using KeyGo.Core.Abstractions;
using KeyGo.Core.Models;

namespace KeyGo.Core.Services;

public sealed class GeminiProvider : IAIProvider
{
    private readonly ICredentialManager _credentialManager;

    public GeminiProvider(ICredentialManager credentialManager)
    {
        _credentialManager = credentialManager;
    }

    public string Id => "gemini";
    public string DisplayName => "Google Gemini";

    public async Task<ProviderConnectionResult> ValidateAsync(CancellationToken cancellationToken = default)
    {
        var apiKey = await _credentialManager.GetAsync(Id, "default", cancellationToken);
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return ProviderConnectionResult.Failed(Id, "No Gemini API key configured.");
        }

        return ProviderConnectionResult.Connected(
            Id,
            "Gemini credential is configured.",
            new[] { "Authentication", "Model discovery" },
            new[] { "Vision", "Audio", "Tool calling" });
    }

    public Task<IReadOnlyList<AIModel>> GetModelsAsync(CancellationToken cancellationToken = default)
    {
        var models = new List<AIModel>
        {
            new()
            {
                Id = "gemini-1.5-flash",
                DisplayName = "Gemini 1.5 Flash",
                Provider = Id,
                ContextLength = 1048576,
                SupportsVision = true,
                SupportsAudio = false,
                SupportsTools = true,
                SupportsImageGeneration = false,
                SupportsReasoning = true,
                Status = "Verified"
            },
            new()
            {
                Id = "gemini-1.5-pro",
                DisplayName = "Gemini 1.5 Pro",
                Provider = Id,
                ContextLength = 1048576,
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
            Content = $"Gemini provider received: {request.Prompt}",
            Provider = Id,
            Model = request.Model ?? "gemini-1.5-flash",
            InputTokens = 110,
            OutputTokens = 90,
            EstimatedCost = 0.0014m,
            Status = "Estimated"
        });
    }

    public async IAsyncEnumerable<AIStreamEvent> StreamAsync(AIRequest request, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var chunks = new[]
        {
            "Gemini: ",
            "Reviewing prompt...",
            "\n",
            request.Prompt
        };

        foreach (var chunk in chunks)
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return new AIStreamEvent { Type = "text", Value = chunk, IsFinal = false };
            await Task.Delay(40, cancellationToken);
        }

        yield return new AIStreamEvent { Type = "text", Value = string.Empty, IsFinal = true };
    }
}
