namespace KeyGo.Core.Models;

public enum ProjectFileKind
{
    Source,
    Test,
    Configuration,
    Documentation,
    Data,
    Asset,
    Generated,
    Binary,
    Unknown
}

public sealed class ProjectWorkspace
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Name { get; init; } = string.Empty;
    public string RootPath { get; init; } = string.Empty;
    public bool IsGitRepository { get; init; }
    public IReadOnlyList<string> Technologies { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> Frameworks { get; init; } = Array.Empty<string>();
    public DateTime OpenedAtUtc { get; init; } = DateTime.UtcNow;
}

public sealed class ProjectFileEntry
{
    public string RelativePath { get; init; } = string.Empty;
    public string FullPath { get; init; } = string.Empty;
    public string Extension { get; init; } = string.Empty;
    public ProjectFileKind Kind { get; init; }
    public long Length { get; init; }
    public int? LineCount { get; init; }
    public bool IsBinary { get; init; }
}

public sealed class ProjectIndexSnapshot
{
    public ProjectWorkspace Project { get; init; } = new();
    public IReadOnlyList<ProjectFileEntry> Files { get; init; } = Array.Empty<ProjectFileEntry>();
    public DateTime IndexedAtUtc { get; init; } = DateTime.UtcNow;
    public int SourceFileCount => Files.Count(file => file.Kind == ProjectFileKind.Source);
    public int TestFileCount => Files.Count(file => file.Kind == ProjectFileKind.Test);
    public int DocumentationFileCount => Files.Count(file => file.Kind == ProjectFileKind.Documentation);
}

public sealed class ProjectSearchResult
{
    public string RelativePath { get; init; } = string.Empty;
    public int LineNumber { get; init; }
    public string Snippet { get; init; } = string.Empty;
    public ProjectFileKind Kind { get; init; }
}

public sealed class ProjectDetectionResult
{
    public IReadOnlyList<string> Technologies { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> Frameworks { get; init; } = Array.Empty<string>();
    public bool IsGitRepository { get; init; }
}
