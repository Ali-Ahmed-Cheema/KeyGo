using KeyGo.Core.Models;
using KeyGo.Core.Services;
using Xunit;

namespace KeyGo.Core.Tests;

public sealed class ProjectContextServiceTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "KeyGoContextTests", Guid.NewGuid().ToString("N"));

    public ProjectContextServiceTests()
    {
        Directory.CreateDirectory(Path.Combine(_root, "src"));
        File.WriteAllText(Path.Combine(_root, "src", "AuthService.cs"), "class AuthService {\n  string token = \"do-not-send\";\n  void Login() { }\n}");
        File.WriteAllText(Path.Combine(_root, "README.md"), "Authentication documentation");
    }

    [Fact]
    public async Task BuildAsync_ExcludesLikelySecretsAndPreservesReferences()
    {
        var workspace = new ProjectWorkspaceService();
        var context = new ProjectContextService(workspace, new ProjectContextOptions { MaximumCharacters = 10000 });

        var result = await context.BuildAsync(_root, "Authentication", cloudRequest: true);

        Assert.Contains("src/AuthService.cs", result.SecretMatches);
        Assert.DoesNotContain(result.IncludedFiles, file => file.RelativePath == "src/AuthService.cs");
        Assert.Contains(result.IncludedFiles, file => file.RelativePath == "README.md");
    }

    [Fact]
    public async Task BuildAsync_RejectsCloudRequestInLocalOnlyMode()
    {
        var workspace = new ProjectWorkspaceService();
        var context = new ProjectContextService(workspace, new ProjectContextOptions { PrivacyMode = ProjectPrivacyMode.LocalOnly });

        await Assert.ThrowsAsync<InvalidOperationException>(() => context.BuildAsync(_root, "Authentication", cloudRequest: true));
    }

    public void Dispose()
    {
        if (Directory.Exists(_root)) Directory.Delete(_root, recursive: true);
    }
}
