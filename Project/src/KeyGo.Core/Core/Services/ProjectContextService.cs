using System.Text;
using System.Text.RegularExpressions;
using KeyGo.Core.Models;

namespace KeyGo.Core.Services;

public sealed class ProjectContextService
{
    private static readonly Regex SecretPattern = new("(?i)(api[_-]?key|secret|password|token|private[_-]?key|connectionstring)\\s*[:=]\\s*[^\\s,;]+", RegexOptions.Compiled);
    private readonly ProjectWorkspaceService _workspace;
    private readonly ProjectContextOptions _options;

    public ProjectContextService(ProjectWorkspaceService workspace, ProjectContextOptions? options = null)
    {
        _workspace = workspace;
        _options = options ?? new ProjectContextOptions();
    }

    public async Task<ProjectContextResult> BuildAsync(string projectPath, string query, bool cloudRequest, CancellationToken cancellationToken = default)
    {
        if (cloudRequest && _options.PrivacyMode == ProjectPrivacyMode.LocalOnly) throw new InvalidOperationException("Cloud AI is disabled for this project.");
        var snapshot = await _workspace.IndexAsync(projectPath, cancellationToken);
        var searchResults = await _workspace.SearchAsync(projectPath, query, cancellationToken);
        var ranked = snapshot.Files
            .Where(file => !file.IsBinary)
            .Select(file => new
            {
                File = file,
                Score = searchResults.Count(result => result.RelativePath.Equals(file.RelativePath, StringComparison.OrdinalIgnoreCase)) * 10
                    + (file.Kind == ProjectFileKind.Source ? 2 : 0)
                    + (file.Kind == ProjectFileKind.Test ? 1 : 0)
            })
            .Where(item => item.Score > 0)
            .OrderByDescending(item => item.Score)
            .ThenBy(item => item.File.RelativePath)
            .ToArray();

        var included = new List<ProjectContextReference>();
        var excluded = new List<string>();
        var secrets = new List<string>();
        var builder = new StringBuilder();
        foreach (var item in ranked)
        {
            var text = await _workspace.ReadFileAsync(projectPath, item.File.RelativePath, cancellationToken);
            if (_options.ScanForSecrets && SecretPattern.IsMatch(text))
            {
                secrets.Add(item.File.RelativePath);
                excluded.Add(item.File.RelativePath);
                continue;
            }

            var block = $"// {item.File.RelativePath}\n{text}\n\n";
            if (builder.Length + block.Length > _options.MaximumCharacters)
            {
                excluded.Add(item.File.RelativePath);
                continue;
            }

            builder.Append(block);
            var matchingLines = searchResults.Where(result => result.RelativePath.Equals(item.File.RelativePath, StringComparison.OrdinalIgnoreCase)).Select(result => result.LineNumber).ToArray();
            included.Add(new ProjectContextReference
            {
                RelativePath = item.File.RelativePath,
                StartLine = matchingLines.Length == 0 ? null : matchingLines.Min(),
                EndLine = matchingLines.Length == 0 ? null : matchingLines.Max(),
                RelevanceScore = item.Score
            });
        }

        return new ProjectContextResult
        {
            Query = query,
            Content = builder.ToString(),
            EstimatedTokens = Math.Max(1, builder.Length / 4),
            IncludedFiles = included,
            ExcludedFiles = excluded,
            SecretMatches = secrets,
            PrivacyMode = _options.PrivacyMode
        };
    }
}
