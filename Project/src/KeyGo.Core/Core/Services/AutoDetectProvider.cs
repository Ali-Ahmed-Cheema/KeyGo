using KeyGo.Core.Abstractions;
using KeyGo.Core.Models;

namespace KeyGo.Core.Services;

public sealed class AutoDetectProvider : IAIProvider
{
    private readonly ICredentialManager _credentials;
    private readonly IReadOnlyList<IAIProvider> _providers;
    private IAIProvider? _detected;

    public AutoDetectProvider(ICredentialManager credentials, IEnumerable<IAIProvider> providers)
    {
        _credentials = credentials;
        _providers = providers.ToArray();
    }

    public string Id => "auto";
    public string DisplayName => "Auto-detect provider";

    public async Task<ProviderConnectionResult> ValidateAsync(CancellationToken cancellationToken = default)
    {
        foreach (var provider in _providers)
        {
            var result = await provider.ValidateAsync(cancellationToken);
            if (result.IsConnected)
            {
                _detected = provider;
                return ProviderConnectionResult.Connected(Id, $"Connected to {provider.DisplayName}.", result.VerifiedCapabilities, result.UnknownCapabilities);
            }
        }

        return ProviderConnectionResult.Failed(Id, "The key did not authenticate with a supported provider. Select a provider or enter its endpoint.");
    }

    public async Task<ProviderCapabilities> GetCapabilitiesAsync(CancellationToken cancellationToken = default)
    {
        var provider = await GetDetectedAsync(cancellationToken);
        return provider is null ? ProviderCapabilities.Create(Id) : await provider.GetCapabilitiesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AIModel>> GetModelsAsync(CancellationToken cancellationToken = default)
    {
        var provider = await GetDetectedAsync(cancellationToken);
        return provider is null ? Array.Empty<AIModel>() : await provider.GetModelsAsync(cancellationToken);
    }

    public async Task<AIResponse> SendAsync(AIRequest request, CancellationToken cancellationToken = default)
    {
        var provider = await GetRequiredProviderAsync(cancellationToken);
        return await provider.SendAsync(new AIRequest { Provider = provider.Id, Model = request.Model, Prompt = request.Prompt, Metadata = request.Metadata }, cancellationToken);
    }

    public async IAsyncEnumerable<AIStreamEvent> StreamAsync(AIRequest request, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var provider = await GetRequiredProviderAsync(cancellationToken);
        await foreach (var item in provider.StreamAsync(new AIRequest { Provider = provider.Id, Model = request.Model, Prompt = request.Prompt, Metadata = request.Metadata }, cancellationToken))
        {
            yield return item;
        }
    }

    private async Task<IAIProvider?> GetDetectedAsync(CancellationToken cancellationToken)
    {
        if (_detected is not null) return _detected;
        var result = await ValidateAsync(cancellationToken);
        return result.IsConnected ? _detected : null;
    }

    private async Task<IAIProvider> GetRequiredProviderAsync(CancellationToken cancellationToken)
        => await GetDetectedAsync(cancellationToken) ?? throw new InvalidOperationException("Connect a provider before chatting.");
}
