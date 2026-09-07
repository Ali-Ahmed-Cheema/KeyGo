namespace KeyGo.Core.Abstractions;

public interface ICredentialStore
{
    Task StoreAsync(string provider, string credentialName, string secretValue, CancellationToken cancellationToken = default);
    Task<string?> GetAsync(string provider, string credentialName, CancellationToken cancellationToken = default);
    Task RemoveAsync(string provider, string credentialName, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> ListCredentialNamesAsync(string provider, CancellationToken cancellationToken = default);
}
