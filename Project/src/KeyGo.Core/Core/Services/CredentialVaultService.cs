using KeyGo.Core.Abstractions;
using KeyGo.Core.Models;

namespace KeyGo.Core.Services;

public interface ICredentialVaultService
{
    Task SaveCredentialAsync(string provider, string alias, string secret, CancellationToken cancellationToken = default);
    Task<string?> GetCredentialAsync(string provider, string alias, CancellationToken cancellationToken = default);
    Task DeleteCredentialAsync(string provider, string alias, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CredentialRecord>> ListCredentialsAsync(string provider, CancellationToken cancellationToken = default);
    string MaskSecret(string secret);
}

public sealed class CredentialVaultService : ICredentialVaultService
{
    private readonly ICredentialManager _credentialManager;

    public CredentialVaultService(ICredentialManager credentialManager)
    {
        _credentialManager = credentialManager;
    }

    public async Task SaveCredentialAsync(string provider, string alias, string secret, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(provider)) throw new ArgumentException("Provider is required.", nameof(provider));
        if (string.IsNullOrWhiteSpace(alias)) throw new ArgumentException("Alias is required.", nameof(alias));
        if (string.IsNullOrWhiteSpace(secret)) throw new ArgumentException("Secret cannot be empty.", nameof(secret));

        await _credentialManager.SaveAsync(provider, alias, secret, cancellationToken);
    }

    public Task<string?> GetCredentialAsync(string provider, string alias, CancellationToken cancellationToken = default)
    {
        return _credentialManager.GetAsync(provider, alias, cancellationToken);
    }

    public Task DeleteCredentialAsync(string provider, string alias, CancellationToken cancellationToken = default)
    {
        return _credentialManager.DeleteAsync(provider, alias, cancellationToken);
    }

    public async Task<IReadOnlyList<CredentialRecord>> ListCredentialsAsync(string provider, CancellationToken cancellationToken = default)
    {
        var aliases = await _credentialManager.ListAliasesAsync(provider, cancellationToken);
        var list = new List<CredentialRecord>();

        foreach (var alias in aliases)
        {
            var secret = await _credentialManager.GetAsync(provider, alias, cancellationToken);
            list.Add(new CredentialRecord
            {
                Provider = provider,
                Alias = alias,
                MaskedValue = MaskSecret(secret ?? string.Empty),
                CreatedAtUtc = DateTime.UtcNow,
                LastUsedUtc = null
            });
        }

        return list;
    }

    public string MaskSecret(string secret)
    {
        if (string.IsNullOrWhiteSpace(secret)) return "";

        if (secret.Length <= 4)
        {
            return new string('*', secret.Length);
        }

        return $"{new string('*', secret.Length - 4)}{secret[^4..]}";
    }
}
