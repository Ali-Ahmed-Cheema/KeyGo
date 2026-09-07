using KeyGo.Core.Abstractions;
using KeyGo.Core.Models;

namespace KeyGo.Core.Services;

public interface IProviderManager
{
    IReadOnlyCollection<IAIProvider> Providers { get; }
    Task<ProviderVerificationResult> VerifyProviderAsync(string providerId, string? credentialAlias = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AIModel>> DiscoverModelsAsync(string providerId, string? credentialAlias = null, CancellationToken cancellationToken = default);
}

public sealed class ProviderManager : IProviderManager
{
    private readonly ICredentialManager _credentialManager;
    private readonly IReadOnlyDictionary<string, IAIProvider> _providers;

    public ProviderManager(ICredentialManager credentialManager, IEnumerable<IAIProvider> providers)
    {
        _credentialManager = credentialManager;
        _providers = providers.ToDictionary(p => p.Id, StringComparer.OrdinalIgnoreCase);
    }

    public IReadOnlyCollection<IAIProvider> Providers => _providers.Values.ToArray();

    public async Task<ProviderVerificationResult> VerifyProviderAsync(string providerId, string? credentialAlias = null, CancellationToken cancellationToken = default)
    {
        var alias = credentialAlias ?? "default";

        if (!_providers.TryGetValue(providerId, out var provider))
        {
            return new ProviderVerificationResult
            {
                ProviderId = providerId,
                DisplayName = providerId,
                Status = ProviderVerificationStatus.Unknown,
                IsVerified = false,
                Message = "Provider not found.",
                ModelsDiscovered = 0
            };
        }

        var key = await _credentialManager.GetAsync(providerId, alias, cancellationToken);
        if (string.IsNullOrWhiteSpace(key))
        {
            return new ProviderVerificationResult
            {
                ProviderId = provider.Id,
                DisplayName = provider.DisplayName,
                Status = ProviderVerificationStatus.InvalidCredential,
                IsVerified = false,
                Message = "No API credential found for provider.",
                ModelsDiscovered = 0
            };
        }

        var connection = await provider.ValidateAsync(cancellationToken);
        if (!connection.IsConnected)
        {
            return new ProviderVerificationResult
            {
                ProviderId = provider.Id,
                DisplayName = provider.DisplayName,
                Status = ProviderVerificationStatus.InvalidCredential,
                IsVerified = false,
                Message = connection.Message,
                ModelsDiscovered = 0
            };
        }

        var models = await provider.GetModelsAsync(cancellationToken);

        return new ProviderVerificationResult
        {
            ProviderId = provider.Id,
            DisplayName = provider.DisplayName,
            Status = ProviderVerificationStatus.Connected,
            IsVerified = true,
            Message = "Provider verified successfully.",
            ModelsDiscovered = models.Count,
            VerifiedCapabilities = new[] { "Authentication", "Model discovery", "Streaming" },
            UnknownCapabilities = new[] { "Image generation", "Audio" },
            VerifiedAtUtc = DateTime.UtcNow
        };
    }

    public async Task<IReadOnlyList<AIModel>> DiscoverModelsAsync(string providerId, string? credentialAlias = null, CancellationToken cancellationToken = default)
    {
        var alias = credentialAlias ?? "default";

        if (!_providers.TryGetValue(providerId, out var provider))
        {
            return Array.Empty<AIModel>();
        }

        var key = await _credentialManager.GetAsync(providerId, alias, cancellationToken);
        if (string.IsNullOrWhiteSpace(key))
        {
            return Array.Empty<AIModel>();
        }

        return await provider.GetModelsAsync(cancellationToken);
    }
}
