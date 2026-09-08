using KeyGo.Core.Abstractions;
using KeyGo.Core.Services;
using KeyGo.Core.Storage;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using System.IO;

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
        ConfigureLocalTestCredential();
        Providers = new IAIProvider[]
        {
            new OpenAIProvider(Credentials, new HttpClient()),
            new GeminiProvider(Credentials),
            new AnthropicProvider(Credentials)
        };
        ProviderManager = new ProviderManager(Credentials, Providers);

        var options = new DbContextOptionsBuilder<KeyGoDbContext>()
            .UseInMemoryDatabase($"keygo-ui-{Guid.NewGuid():N}")
            .Options;
        var database = new KeyGoDbContext(options);
        Conversations = new ConversationService(database);
        Chat = new WorkspaceChatService(new ProjectContextService(Workspace), Providers, Conversations);
        Agent = new CodingAgentOrchestrator(Workspace, new ProjectContextService(Workspace));
    }

    private void ConfigureLocalTestCredential()
    {
        var keyPath = Path.Combine(AppContext.BaseDirectory, "APIKey.txt");
        if (!File.Exists(keyPath))
        {
            keyPath = Path.Combine(Directory.GetCurrentDirectory(), "APIKey.txt");
        }

        if (!File.Exists(keyPath))
        {
            keyPath = Path.Combine(Directory.GetCurrentDirectory(), "KeyGo", "APIKey.txt");
        }

        if (!File.Exists(keyPath)) return;

        var key = File.ReadAllText(keyPath).Trim();
        if (!string.IsNullOrWhiteSpace(key))
        {
            Credentials.SaveAsync("gemini", "default", key).GetAwaiter().GetResult();
        }
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