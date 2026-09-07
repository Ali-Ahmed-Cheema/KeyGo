namespace KeyGo.Core.Abstractions;

using KeyGo.Core.Models;

public interface IProjectService
{
    Task<string> OpenProjectAsync(string path, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<string>> GetFilesAsync(string projectPath, CancellationToken cancellationToken = default);
    ProjectDetectionResult Detect(string path);
    Task<ProjectIndexSnapshot> IndexAsync(string path, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProjectSearchResult>> SearchAsync(string path, string query, CancellationToken cancellationToken = default);
    Task<string> ReadFileAsync(string path, string relativePath, CancellationToken cancellationToken = default);
}
