namespace KeyGo.Core.Abstractions;

public interface ICredentialManager
{
    Task SaveAsync(string provider, string alias, string secret, CancellationToken cancellationToken = default);
    Task<string?> GetAsync(string provider, string alias, CancellationToken cancellationToken = default);
    Task DeleteAsync(string provider, string alias, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> ListAliasesAsync(string provider, CancellationToken cancellationToken = default);
}
