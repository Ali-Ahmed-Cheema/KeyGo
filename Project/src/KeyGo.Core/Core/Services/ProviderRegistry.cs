using KeyGo.Core.Abstractions;

namespace KeyGo.Core.Services;

public sealed class ProviderRegistry
{
    private readonly IReadOnlyDictionary<string, IAIProvider> _providers;

    public ProviderRegistry(IEnumerable<IAIProvider> providers)
    {
        _providers = providers.ToDictionary(p => p.Id, StringComparer.OrdinalIgnoreCase);
    }

    public IAIProvider Get(string providerId)
    {
        if (_providers.TryGetValue(providerId, out var provider))
        {
            return provider;
        }

        throw new InvalidOperationException($"Provider '{providerId}' is not registered.");
    }

    public IReadOnlyCollection<IAIProvider> All => _providers.Values.ToArray();
}
