using System.Collections.Concurrent;
using System.Text;
using KeyGo.Core.Abstractions;
using KeyGo.Core.Models;

namespace KeyGo.Core.Services;

public sealed class ProjectWorkspaceOptions
{
    public HashSet<string> IgnoredDirectories { get; } = new(StringComparer.OrdinalIgnoreCase)
    {
        ".git", "node_modules", "bin", "obj", "dist", "build", ".cache", "coverage", "venv", "__pycache__"
    };

    public HashSet<string> IgnoredExtensions { get; } = new(StringComparer.OrdinalIgnoreCase)
    {
        ".dll", ".exe", ".pdb", ".zip", ".7z", ".rar", ".jpg", ".jpeg", ".png", ".gif", ".webp", ".ico", ".pdf"
    };

    public long MaximumFileSizeBytes { get; init; } = 2 * 1024 * 1024;
}

public sealed class ProjectWorkspaceService : IProjectService
{
    private static readonly string[] SourceExtensions = [".cs", ".js", ".jsx", ".ts", ".tsx", ".py", ".rs", ".java", ".cpp", ".c", ".h", ".hpp", ".go", ".rb", ".php", ".swift", ".kt"];
    private static readonly string[] TestMarkers = ["test", "tests", "spec", "specs"];
    private readonly ProjectWorkspaceOptions _options;
    private readonly ConcurrentDictionary<string, ProjectIndexSnapshot> _indexes = new(StringComparer.OrdinalIgnoreCase);

    public ProjectWorkspaceService(ProjectWorkspaceOptions? options = null)
    {
        _options = options ?? new ProjectWorkspaceOptions();
    }

    public Task<string> OpenProjectAsync(string path, CancellationToken cancellationToken = default)
    {
        var root = NormalizeRoot(path);
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(root);
    }

    public Task<IReadOnlyCollection<string>> GetFilesAsync(string projectPath, CancellationToken cancellationToken = default)
    {
        var snapshot = IndexAsync(projectPath, cancellationToken);
        return snapshot.ContinueWith(task => (IReadOnlyCollection<string>)task.Result.Files.Select(file => file.FullPath).ToArray(), cancellationToken);
    }

    public async Task<ProjectIndexSnapshot> IndexAsync(string path, CancellationToken cancellationToken = default)
    {
        var root = NormalizeRoot(path);
        var detection = Detect(root);
        var files = new List<ProjectFileEntry>();

        foreach (var file in Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var relativePath = Path.GetRelativePath(root, file);
            if (ShouldIgnore(relativePath))
            {
                continue;
            }

            var info = new FileInfo(file);
            if (info.Length > _options.MaximumFileSizeBytes)
            {
                continue;
            }

            var extension = info.Extension;
            var binary = IsBinary(file, info.Length);
            files.Add(new ProjectFileEntry
            {
                RelativePath = relativePath.Replace(Path.DirectorySeparatorChar, '/'),
                FullPath = file,
                Extension = extension,
                Kind = Classify(relativePath, extension, binary),
                Length = info.Length,
                IsBinary = binary,
                LineCount = binary ? null : await CountLinesAsync(file, cancellationToken)
            });
        }

        var project = new ProjectWorkspace
        {
            Name = new DirectoryInfo(root).Name,
            RootPath = root,
            IsGitRepository = detection.IsGitRepository,
            Technologies = detection.Technologies,
            Frameworks = detection.Frameworks
        };
        var snapshot = new ProjectIndexSnapshot { Project = project, Files = files.OrderBy(file => file.RelativePath).ToArray() };
        _indexes[root] = snapshot;
        return snapshot;
    }

    public ProjectDetectionResult Detect(string path)
    {
        var root = NormalizeRoot(path);
        var files = Directory.EnumerateFiles(root, "*", SearchOption.TopDirectoryOnly).Select(Path.GetFileName).Where(name => name is not null).Cast<string>().ToHashSet(StringComparer.OrdinalIgnoreCase);
        var technologies = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var frameworks = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (files.Any(file => file.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase) || file.EndsWith(".sln", StringComparison.OrdinalIgnoreCase))) technologies.Add(".NET");
        if (files.Contains("package.json")) technologies.Add("Node.js");
        if (files.Contains("requirements.txt") || files.Contains("pyproject.toml")) technologies.Add("Python");
        if (files.Contains("Cargo.toml")) technologies.Add("Rust");
        if (files.Contains("pom.xml") || files.Contains("build.gradle")) technologies.Add("Java");
        if (files.Contains("CMakeLists.txt")) technologies.Add("C/C++");
        if (files.Contains("Makefile")) technologies.Add("Make");
        if (files.Any(file => file.StartsWith("vite.config.", StringComparison.OrdinalIgnoreCase))) frameworks.Add("Vite");
        if (files.Any(file => file.StartsWith("next.config.", StringComparison.OrdinalIgnoreCase))) frameworks.Add("Next.js");
        if (files.Any(file => file.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase))) frameworks.Add(".NET project");

        return new ProjectDetectionResult
        {
            Technologies = technologies.OrderBy(value => value).ToArray(),
            Frameworks = frameworks.OrderBy(value => value).ToArray(),
            IsGitRepository = Directory.Exists(Path.Combine(root, ".git"))
        };
    }

    public async Task<IReadOnlyList<ProjectSearchResult>> SearchAsync(string path, string query, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query)) return Array.Empty<ProjectSearchResult>();
        var snapshot = _indexes.TryGetValue(NormalizeRoot(path), out var existing) ? existing : await IndexAsync(path, cancellationToken);
        var results = new List<ProjectSearchResult>();
        foreach (var file in snapshot.Files.Where(file => !file.IsBinary))
        {
            cancellationToken.ThrowIfCancellationRequested();
            foreach (var match in await SearchFileAsync(file, query, cancellationToken)) results.Add(match);
        }
        return results;
    }

    public async Task<string> ReadFileAsync(string path, string relativePath, CancellationToken cancellationToken = default)
    {
        var root = NormalizeRoot(path);
        var fullPath = EnsureInsideRoot(root, relativePath);
        if (new FileInfo(fullPath).Length > _options.MaximumFileSizeBytes) throw new IOException("File exceeds the configured read limit.");
        return await File.ReadAllTextAsync(fullPath, Encoding.UTF8, cancellationToken);
    }

    private bool ShouldIgnore(string relativePath)
    {
        var segments = relativePath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        return segments.Any(segment => _options.IgnoredDirectories.Contains(segment)) || _options.IgnoredExtensions.Contains(Path.GetExtension(relativePath));
    }

    private static ProjectFileKind Classify(string relativePath, string extension, bool binary)
    {
        if (binary) return ProjectFileKind.Binary;
        var fileName = Path.GetFileName(relativePath);
        if (TestMarkers.Any(marker => relativePath.Split('/', '\\').Any(segment => string.Equals(segment, marker, StringComparison.OrdinalIgnoreCase))) || fileName.Contains("test", StringComparison.OrdinalIgnoreCase) || fileName.Contains("spec", StringComparison.OrdinalIgnoreCase)) return ProjectFileKind.Test;
        if (fileName.Equals("README.md", StringComparison.OrdinalIgnoreCase) || fileName.Equals("CHANGELOG.md", StringComparison.OrdinalIgnoreCase) || extension.Equals(".md", StringComparison.OrdinalIgnoreCase)) return ProjectFileKind.Documentation;
        if (extension is ".json" or ".xml" or ".yaml" or ".yml" or ".toml" or ".ini" or ".config" || fileName.StartsWith(".env", StringComparison.OrdinalIgnoreCase)) return ProjectFileKind.Configuration;
        if (SourceExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase)) return ProjectFileKind.Source;
        if (extension is ".csv" or ".sql") return ProjectFileKind.Data;
        return ProjectFileKind.Unknown;
    }

    private static bool IsBinary(string path, long length)
    {
        if (length == 0) return false;
        using var stream = File.OpenRead(path);
        var buffer = new byte[Math.Min(4096, (int)length)];
        var read = stream.Read(buffer, 0, buffer.Length);
        return buffer.Take(read).Any(value => value == 0);
    }

    private static async Task<int> CountLinesAsync(string path, CancellationToken cancellationToken)
    {
        var count = 0;
        using var reader = new StreamReader(path, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        while (await reader.ReadLineAsync(cancellationToken) is not null) count++;
        return count;
    }

    private static async Task<IReadOnlyList<ProjectSearchResult>> SearchFileAsync(ProjectFileEntry file, string query, CancellationToken cancellationToken)
    {
        var results = new List<ProjectSearchResult>();
        using var reader = new StreamReader(file.FullPath, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        var lineNumber = 0;
        while (await reader.ReadLineAsync(cancellationToken) is { } line)
        {
            lineNumber++;
            if (line.Contains(query, StringComparison.OrdinalIgnoreCase)) results.Add(new ProjectSearchResult { RelativePath = file.RelativePath, LineNumber = lineNumber, Snippet = line.Trim(), Kind = file.Kind });
        }
        return results;
    }

    private static string NormalizeRoot(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("Project path is required.", nameof(path));
        var root = Path.GetFullPath(path);
        if (!Directory.Exists(root)) throw new DirectoryNotFoundException(root);
        return root.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    }

    private static string EnsureInsideRoot(string root, string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath) || Path.IsPathRooted(relativePath)) throw new UnauthorizedAccessException("Only relative project paths are allowed.");
        var fullPath = Path.GetFullPath(Path.Combine(root, relativePath));
        var prefix = root.EndsWith(Path.DirectorySeparatorChar) ? root : root + Path.DirectorySeparatorChar;
        if (!fullPath.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) throw new UnauthorizedAccessException("Path is outside the project root.");
        if (!File.Exists(fullPath)) throw new FileNotFoundException("Project file was not found.", relativePath);
        return fullPath;
    }
}
