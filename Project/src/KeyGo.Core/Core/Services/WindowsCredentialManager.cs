using KeyGo.Core.Abstractions;

namespace KeyGo.Core.Services;

public sealed class WindowsCredentialManager : ICredentialManager
{
    private readonly Dictionary<string, string> _store = new(StringComparer.OrdinalIgnoreCase);

    public Task SaveAsync(string provider, string alias, string secret, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _store[$"{provider}:{alias}"] = secret;
        return Task.CompletedTask;
    }

    public Task<string?> GetAsync(string provider, string alias, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(_store.TryGetValue($"{provider}:{alias}", out var value) ? value : null);
    }

    public Task DeleteAsync(string provider, string alias, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _store.Remove($"{provider}:{alias}");
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<string>> ListAliasesAsync(string provider, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var items = _store
            .Keys
            .Where(k => k.StartsWith($"{provider}:", StringComparison.OrdinalIgnoreCase))
            .Select(k => k[(provider.Length + 1)..])
            .ToList();

        return Task.FromResult<IReadOnlyList<string>>(items);
    }
}
