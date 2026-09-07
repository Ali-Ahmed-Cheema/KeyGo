using KeyGo.Core.Abstractions;

namespace KeyGo.Core.Services;

public sealed class ModelRouter : IModelRouter
{
    public Task<string> SelectModelAsync(string taskDescription, IReadOnlyCollection<IModel> availableModels, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (availableModels.Count == 0)
        {
            throw new InvalidOperationException("No models are available for routing.");
        }

        var best = availableModels
            .OrderByDescending(model => model.Capabilities.Count)
            .ThenBy(model => model.Provider)
            .First();

        return Task.FromResult(best.Id);
    }
}
