namespace KeyGo.Core.Models;

public enum ProjectPrivacyMode
{
    Cloud,
    LocalOnly,
    Hybrid
}

public sealed class ProjectContextOptions
{
    public int MaximumCharacters { get; init; } = 24000;
    public ProjectPrivacyMode PrivacyMode { get; init; } = ProjectPrivacyMode.Cloud;
    public bool ScanForSecrets { get; init; } = true;
}

public sealed class ProjectContextReference
{
    public string RelativePath { get; init; } = string.Empty;
    public int? StartLine { get; init; }
    public int? EndLine { get; init; }
    public int RelevanceScore { get; init; }
}

public sealed class ProjectContextResult
{
    public string Query { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public int EstimatedTokens { get; init; }
    public IReadOnlyList<ProjectContextReference> IncludedFiles { get; init; } = Array.Empty<ProjectContextReference>();
    public IReadOnlyList<string> ExcludedFiles { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> SecretMatches { get; init; } = Array.Empty<string>();
    public ProjectPrivacyMode PrivacyMode { get; init; }
}
