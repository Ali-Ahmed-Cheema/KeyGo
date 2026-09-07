using KeyGo.Core.Services;
using Xunit;

namespace KeyGo.Core.Tests;

public sealed class ProjectWorkspaceServiceTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "KeyGoTests", Guid.NewGuid().ToString("N"));

    public ProjectWorkspaceServiceTests()
    {
        Directory.CreateDirectory(Path.Combine(_root, "src"));
        Directory.CreateDirectory(Path.Combine(_root, "tests"));
        Directory.CreateDirectory(Path.Combine(_root, "node_modules"));
        Directory.CreateDirectory(Path.Combine(_root, ".git"));
        File.WriteAllText(Path.Combine(_root, "package.json"), "{\"name\":\"fixture\"}");
        File.WriteAllText(Path.Combine(_root, "src", "AuthService.cs"), "class AuthService {\n  // authentication\n}");
        File.WriteAllText(Path.Combine(_root, "tests", "AuthTests.cs"), "class AuthTests { }");
        File.WriteAllText(Path.Combine(_root, "README.md"), "Project documentation");
        File.WriteAllText(Path.Combine(_root, "node_modules", "ignored.js"), "authentication");
    }

    [Fact]
    public async Task IndexAsync_DetectsTechnologyAndExcludesIgnoredDirectories()
    {
        var service = new ProjectWorkspaceService();

        var snapshot = await service.IndexAsync(_root);

        Assert.Contains("Node.js", snapshot.Project.Technologies);
        Assert.True(snapshot.Project.IsGitRepository);
        Assert.Contains(snapshot.Files, file => file.RelativePath == "src/AuthService.cs" && file.Kind == KeyGo.Core.Models.ProjectFileKind.Source);
        Assert.Contains(snapshot.Files, file => file.RelativePath == "tests/AuthTests.cs" && file.Kind == KeyGo.Core.Models.ProjectFileKind.Test);
        Assert.DoesNotContain(snapshot.Files, file => file.RelativePath.Contains("node_modules", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task SearchAsync_ReturnsFileAndLineForMatchingCode()
    {
        var service = new ProjectWorkspaceService();

        var results = await service.SearchAsync(_root, "authentication");

        var result = Assert.Single(results);
        Assert.Equal("src/AuthService.cs", result.RelativePath);
        Assert.Equal(2, result.LineNumber);
    }

    [Fact]
    public async Task ReadFileAsync_RejectsPathTraversal()
    {
        var service = new ProjectWorkspaceService();

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.ReadFileAsync(_root, "../outside.txt"));
    }

    public void Dispose()
    {
        if (Directory.Exists(_root)) Directory.Delete(_root, recursive: true);
    }
}
