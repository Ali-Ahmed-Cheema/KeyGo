namespace KeyGo.Core.Abstractions;

public interface IModelRouter
{
    Task<string> SelectModelAsync(string taskDescription, IReadOnlyCollection<IModel> availableModels, CancellationToken cancellationToken = default);
}
