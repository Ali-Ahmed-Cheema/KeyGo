using KeyGo.Core.Models;
using KeyGo.Core.Services;
using Xunit;

namespace KeyGo.Core.Tests;

public sealed class FileChangeServiceTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "KeyGoChangeTests", Guid.NewGuid().ToString("N"));

    public FileChangeServiceTests()
    {
        Directory.CreateDirectory(_root);
        File.WriteAllText(Path.Combine(_root, "source.cs"), "old content");
    }

    [Fact]
    public async Task ApplyAsync_RequiresApprovalAndWritePermission()
    {
        var permissions = new AgentPermissionService(AgentMode.Assist);
        var service = new FileChangeService(permissions);
        var proposal = CreateProposal(ApprovalState.Pending);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.ApplyAsync(_root, proposal));
        proposal.Approval = ApprovalState.Approved;
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.ApplyAsync(_root, proposal));
    }

    [Fact]
    public async Task ApplyAsync_WritesApprovedChangeAndProducesDiff()
    {
        var permissions = new AgentPermissionService(AgentMode.Assist);
        permissions.GrantForSession(AgentPermission.Write);
        var service = new FileChangeService(permissions);
        var proposal = CreateProposal(ApprovalState.Approved);

        var review = service.CreateReview(proposal);
        await service.ApplyAsync(_root, proposal);

        Assert.Contains("- old content", review.Diffs.Single().UnifiedText);
        Assert.Contains("+ new content", review.Diffs.Single().UnifiedText);
        Assert.Equal("new content", await File.ReadAllTextAsync(Path.Combine(_root, "source.cs")));
    }

    [Fact]
    public async Task ApplyAsync_RejectsTraversalEvenWithPermission()
    {
        var permissions = new AgentPermissionService(AgentMode.Assist);
        permissions.GrantForSession(AgentPermission.Write);
        var service = new FileChangeService(permissions);
        var proposal = new FileChangeProposal
        {
            Approval = ApprovalState.Approved,
            Changes = new[] { new ProposedFileChange { RelativePath = "../outside.txt", ProposedContent = "blocked" } }
        };

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.ApplyAsync(_root, proposal));
    }

    [Fact]
    public async Task ApplyAsync_RejectsStaleProposalInsteadOfOverwritingUserChanges()
    {
        var permissions = new AgentPermissionService(AgentMode.Assist);
        permissions.GrantForSession(AgentPermission.Write);
        var service = new FileChangeService(permissions);
        var proposal = CreateProposal(ApprovalState.Approved);
        await File.WriteAllTextAsync(Path.Combine(_root, "source.cs"), "user change");

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.ApplyAsync(_root, proposal));
        Assert.Equal("user change", await File.ReadAllTextAsync(Path.Combine(_root, "source.cs")));
    }

    private FileChangeProposal CreateProposal(ApprovalState approval) => new()
    {
        Approval = approval,
        Goal = "Update source",
        Changes = new[] { new ProposedFileChange { RelativePath = "source.cs", OriginalContent = "old content", ProposedContent = "new content" } }
    };

    public void Dispose()
    {
        if (Directory.Exists(_root)) Directory.Delete(_root, recursive: true);
    }
}
