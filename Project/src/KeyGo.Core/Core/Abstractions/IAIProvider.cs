namespace KeyGo.Core.Abstractions;

public interface IAIProvider
{
    string ProviderName { get; }
    Task<bool> ValidateAsync(CancellationToken cancellationToken = default);
    Task<string> SendMessageAsync(string message, CancellationToken cancellationToken = default);
    IAsyncEnumerable<string> StreamMessageAsync(string message, CancellationToken cancellationToken = default);
}
