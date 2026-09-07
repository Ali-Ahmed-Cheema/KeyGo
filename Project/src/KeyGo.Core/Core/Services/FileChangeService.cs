using System.Text;
using KeyGo.Core.Abstractions;
using KeyGo.Core.Models;

namespace KeyGo.Core.Services;

public sealed class FileChangeService
{
    private readonly IPermissionService _permissions;
    private readonly IAuditLogger? _auditLogger;

    public FileChangeService(IPermissionService permissions, IAuditLogger? auditLogger = null)
    {
        _permissions = permissions;
        _auditLogger = auditLogger;
    }

    public ChangeReview CreateReview(FileChangeProposal proposal)
    {
        var diffs = proposal.Changes.Select(change => new FileDiff
        {
            RelativePath = change.RelativePath,
            UnifiedText = BuildDiff(change)
        }).ToArray();
        return new ChangeReview { Diffs = diffs, LinesAdded = proposal.LinesAdded, LinesRemoved = proposal.LinesRemoved };
    }

    public async Task ApplyAsync(string projectRoot, FileChangeProposal proposal, CancellationToken cancellationToken = default)
    {
        if (proposal.Approval != ApprovalState.Approved) throw new InvalidOperationException("File changes require explicit approval.");
        if (!await _permissions.CanExecuteAsync("Write", projectRoot, cancellationToken)) throw new UnauthorizedAccessException("Write permission has not been granted.");

        var root = Path.GetFullPath(projectRoot).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var originals = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        try
        {
            foreach (var change in proposal.Changes)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var fullPath = EnsureInsideRoot(root, change.RelativePath);
                originals[fullPath] = File.Exists(fullPath) ? await File.ReadAllTextAsync(fullPath, cancellationToken) : null;
                if (change.OriginalContent is not null && !string.Equals(originals[fullPath], change.OriginalContent, StringComparison.Ordinal))
                {
                    throw new InvalidOperationException($"File changed after the proposal was generated: {change.RelativePath}");
                }
                Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
                var temporaryPath = fullPath + $".keygo-{Guid.NewGuid():N}.tmp";
                await File.WriteAllTextAsync(temporaryPath, change.ProposedContent, Encoding.UTF8, cancellationToken);
                File.Move(temporaryPath, fullPath, overwrite: true);
            }
            if (_auditLogger is not null) await _auditLogger.LogAsync($"Applied approved file proposal {proposal.Id} affecting {proposal.FilesChanged} file(s).", cancellationToken);
        }
        catch
        {
            foreach (var original in originals)
            {
                if (original.Value is null) File.Delete(original.Key);
                else await File.WriteAllTextAsync(original.Key, original.Value, Encoding.UTF8, cancellationToken);
            }
            throw;
        }
    }

    private static string BuildDiff(ProposedFileChange change)
    {
        var oldLines = (change.OriginalContent ?? string.Empty).Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n');
        var newLines = change.ProposedContent.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n');
        var builder = new StringBuilder();
        builder.AppendLine($"--- a/{change.RelativePath}");
        builder.AppendLine($"+++ b/{change.RelativePath}");
        foreach (var line in oldLines) builder.AppendLine($"- {line}");
        foreach (var line in newLines) builder.AppendLine($"+ {line}");
        return builder.ToString();
    }

    private static string EnsureInsideRoot(string root, string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath) || Path.IsPathRooted(relativePath)) throw new UnauthorizedAccessException("Only relative project paths are allowed.");
        var fullPath = Path.GetFullPath(Path.Combine(root, relativePath));
        var prefix = root + Path.DirectorySeparatorChar;
        if (!fullPath.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) throw new UnauthorizedAccessException("Path is outside the project root.");
        return fullPath;
    }
}
