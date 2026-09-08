using KeyGo.Core.Abstractions;
using KeyGo.Core.Models;
using KeyGo.Core.Services;
using KeyGo.Core.Storage;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace KeyGo.Core.Tests;

public sealed class WorkspaceChatServiceTests
{
    [Fact]
    public async Task SendAsync_BuildsContextStreamsResponseAndPersistsMessages()
    {
        var projectRoot = Directory.CreateTempSubdirectory("keygo-chat-");
        try
        {
            await File.WriteAllTextAsync(Path.Combine(projectRoot.FullName, "AuthService.cs"), "public sealed class AuthService { public bool Login() => true; }");
            var workspace = new ProjectWorkspaceService();
            var provider = new FakeProvider();
            var options = new DbContextOptionsBuilder<KeyGoDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            await using var db = new KeyGoDbContext(options);
            var conversations = new ConversationService(db);
            var chat = new WorkspaceChatService(new ProjectContextService(workspace), new[] { provider }, conversations);
            var streamed = string.Empty;

            var result = await chat.SendAsync(projectRoot.FullName, provider.Id, "fake-model", "Why is login broken?", onText: text => streamed += text);
            var saved = await conversations.GetConversationAsync(result.Conversation.Id);

            Assert.Contains("AuthService.cs", result.Context.Content);
            Assert.Equal("fake response", streamed);
            Assert.NotNull(saved);
            Assert.Equal(2, saved!.Messages.Count);
            Assert.Equal("assistant", saved.Messages[1].Role);
            Assert.Equal("fake response", saved.Messages[1].Content);
        }
        finally
        {
            projectRoot.Delete(true);
        }
    }

    private sealed class FakeProvider : IAIProvider
    {
        public string Id => "fake";
        public string DisplayName => "Fake";
        public Task<ProviderConnectionResult> ValidateAsync(CancellationToken cancellationToken = default) => Task.FromResult(ProviderConnectionResult.Connected(Id, "ok", Array.Empty<string>(), Array.Empty<string>()));
        public Task<ProviderCapabilities> GetCapabilitiesAsync(CancellationToken cancellationToken = default) => Task.FromResult(ProviderCapabilities.Create(Id, new[] { ProviderCapability.Text, ProviderCapability.Streaming }));
        public Task<IReadOnlyList<AIModel>> GetModelsAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<AIModel>>(Array.Empty<AIModel>());
        public Task<AIResponse> SendAsync(AIRequest request, CancellationToken cancellationToken = default) => Task.FromResult(new AIResponse { Content = "fake response", Provider = Id, Model = request.Model });

        public async IAsyncEnumerable<AIStreamEvent> StreamAsync(AIRequest request, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            yield return new AIStreamEvent { Type = "text", Value = "fake " };
            await Task.Yield();
            cancellationToken.ThrowIfCancellationRequested();
            yield return new AIStreamEvent { Type = "text", Value = "response", IsFinal = true };
        }
    }
}