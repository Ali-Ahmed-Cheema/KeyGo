using KeyGo.Core.Abstractions;

namespace KeyGo.Core.Services;

public sealed class InMemoryCredentialStore : ICredentialStore
{
    private readonly Dictionary<string, string> _credentials = new(StringComparer.OrdinalIgnoreCase);

    public Task StoreAsync(string provider, string credentialName, string secretValue, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _credentials[$"{provider}:{credentialName}"] = secretValue;
        return Task.CompletedTask;
    }

    public Task<string?> GetAsync(string provider, string credentialName, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(_credentials.TryGetValue($"{provider}:{credentialName}", out var value) ? value : null);
    }

    public Task RemoveAsync(string provider, string credentialName, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _credentials.Remove($"{provider}:{credentialName}");
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<string>> ListCredentialNamesAsync(string provider, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var names = _credentials
            .Keys
            .Where(k => k.StartsWith($"{provider}:", StringComparison.OrdinalIgnoreCase))
            .Select(k => k[(provider.Length + 1)..])
            .ToList();

        return Task.FromResult<IReadOnlyList<string>>(names);
    }
}
