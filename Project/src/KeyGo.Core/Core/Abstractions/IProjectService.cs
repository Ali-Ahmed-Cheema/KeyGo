namespace KeyGo.Core.Abstractions;

public interface IProjectService
{
    Task<string> OpenProjectAsync(string path, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<string>> GetFilesAsync(string projectPath, CancellationToken cancellationToken = default);
}
