using KeyGo.Core.Abstractions;
using KeyGo.Core.Services;
using KeyGo.Core.Storage;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;

namespace KeyGo.App.Services;

public sealed class AppHost
{
    public ProjectWorkspaceService Workspace { get; } = new();
    public WindowsCredentialManager Credentials { get; } = new();
    public IReadOnlyList<IAIProvider> Providers { get; }
    public IProviderManager ProviderManager { get; }
    public WorkspaceChatService Chat { get; }
    public CodingAgentOrchestrator Agent { get; }
    public IConversationService Conversations { get; }

    public AppHost()
    {
        var directProviders = new IAIProvider[]
        {
            new OpenAIProvider(Credentials, new HttpClient()),
            new GeminiProvider(Credentials),
            new AnthropicProvider(Credentials),
            new OpenAICompatibleProvider(Credentials)
        };
        Providers = new IAIProvider[] { new AutoDetectProvider(Credentials, directProviders) }.Concat(directProviders).ToArray();
        ProviderManager = new ProviderManager(Credentials, Providers);

        var options = new DbContextOptionsBuilder<KeyGoDbContext>()
            .UseInMemoryDatabase($"keygo-ui-{Guid.NewGuid():N}")
            .Options;
        var database = new KeyGoDbContext(options);
        Conversations = new ConversationService(database);
        Chat = new WorkspaceChatService(new ProjectContextService(Workspace), Providers, Conversations);
        Agent = new CodingAgentOrchestrator(Workspace, new ProjectContextService(Workspace));
    }

}

public interface IFolderPickerService
{
    string? PickFolder();
}

public sealed class WindowsFolderPickerService : IFolderPickerService
{
    public string? PickFolder()
    {
        using var dialog = new System.Windows.Forms.FolderBrowserDialog
        {
            Description = "Choose a project folder",
            UseDescriptionForTitle = true,
            ShowNewFolderButton = false
        };
        return dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK ? dialog.SelectedPath : null;
    }
}