using KeyGo.Core.Models;

namespace KeyGo.Core.Abstractions;

public interface IAIProvider
{
    string Id { get; }
    string DisplayName { get; }

    Task<ProviderConnectionResult> ValidateAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AIModel>> GetModelsAsync(CancellationToken cancellationToken = default);

    Task<AIResponse> SendAsync(AIRequest request, CancellationToken cancellationToken = default);

    IAsyncEnumerable<AIStreamEvent> StreamAsync(AIRequest request, CancellationToken cancellationToken = default);
}
