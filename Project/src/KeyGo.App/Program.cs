using KeyGo.Core.Services;
using KeyGo.Core.Storage;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;

namespace KeyGo.App;

public static class Program
{
    public static async Task Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: KeyGo.App <project-folder> [question]");
            return;
        }

        var workspace = new ProjectWorkspaceService();
        var snapshot = await workspace.IndexAsync(args[0]);
        Console.WriteLine($"Project: {snapshot.Project.Name}");
        Console.WriteLine($"Files indexed: {snapshot.Files.Count}");
        Console.WriteLine($"Technologies: {string.Join(", ", snapshot.Project.Technologies)}");

        if (args.Length == 1)
        {
            Console.WriteLine("Ask a question by adding it as the second argument.");
            return;
        }

        var credentials = new WindowsCredentialManager();
        var apiKey = Environment.GetEnvironmentVariable("KEYGO_OPENAI_API_KEY");
        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            await credentials.SaveAsync("openai", "default", apiKey);
        }

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            Console.WriteLine("Set KEYGO_OPENAI_API_KEY to start a provider-backed chat.");
            return;
        }

        var provider = new OpenAIProvider(credentials, new HttpClient());
        var context = new ProjectContextService(workspace);
        var options = new DbContextOptionsBuilder<KeyGoDbContext>()
            .UseInMemoryDatabase($"keygo-{Guid.NewGuid():N}")
            .Options;

        await using var db = new KeyGoDbContext(options);
        var conversations = new ConversationService(db);
        var chat = new WorkspaceChatService(context, new[] { provider }, conversations);
        var result = await chat.SendAsync(args[0], provider.Id, "gpt-4o-mini", string.Join(' ', args.Skip(1)), onText: Console.Write);

        Console.WriteLine();
        Console.WriteLine($"Context: {result.Context.IncludedFiles.Count} files, ~{result.Context.EstimatedTokens} tokens");
        Console.WriteLine($"Conversation: {result.Conversation.Id}");
    }
}
