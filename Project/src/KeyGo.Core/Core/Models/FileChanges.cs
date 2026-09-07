namespace KeyGo.Core.Models;

public sealed class ProposedFileChange
{
    public string RelativePath { get; init; } = string.Empty;
    public string? OriginalContent { get; init; }
    public string ProposedContent { get; init; } = string.Empty;
    public bool IsNewFile => OriginalContent is null;
}

public sealed class FileChangeProposal
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Goal { get; init; } = string.Empty;
    public IReadOnlyList<ProposedFileChange> Changes { get; init; } = Array.Empty<ProposedFileChange>();
    public ApprovalState Approval { get; set; }
    public int FilesChanged => Changes.Count;
    public int LinesAdded => Changes.Sum(change => CountLines(change.ProposedContent));
    public int LinesRemoved => Changes.Sum(change => CountLines(change.OriginalContent));

    private static int CountLines(string? content) => string.IsNullOrEmpty(content) ? 0 : content.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n').Length;
}

public sealed class FileDiff
{
    public string RelativePath { get; init; } = string.Empty;
    public string UnifiedText { get; init; } = string.Empty;
}

public sealed class ChangeReview
{
    public IReadOnlyList<FileDiff> Diffs { get; init; } = Array.Empty<FileDiff>();
    public int FilesChanged => Diffs.Count;
    public int LinesAdded { get; init; }
    public int LinesRemoved { get; init; }
}
