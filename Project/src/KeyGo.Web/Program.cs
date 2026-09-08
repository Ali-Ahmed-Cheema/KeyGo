using System.Collections.Concurrent;
using KeyGo.Core.Abstractions;
using KeyGo.Core.Models;
using KeyGo.Core.Services;
using KeyGo.Core.Storage;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<WebRuntime>();
builder.Services.AddCors(options => options.AddDefaultPolicy(policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();
app.UseCors();
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/api/providers", (WebRuntime runtime) => runtime.ProviderCards());

app.MapPost("/api/connect", async (ConnectRequest request, WebRuntime runtime, CancellationToken cancellationToken) =>
{
    if (string.IsNullOrWhiteSpace(request.ApiKey)) return Results.BadRequest(new { message = "Enter an API key." });
    var result = await runtime.ConnectAsync(request.ApiKey, request.ProviderId, request.Endpoint, cancellationToken);
    return result.IsVerified ? Results.Ok(result) : Results.BadRequest(result);
});

app.MapGet("/api/models", (string? providerId, WebRuntime runtime) => Results.Ok(runtime.Models(providerId)));

app.MapPost("/api/project", async (ProjectRequest request, WebRuntime runtime, CancellationToken cancellationToken) =>
{
    try
    {
        var snapshot = await runtime.OpenProjectAsync(request.Path, cancellationToken);
        return Results.Ok(new { project = snapshot.Project, files = snapshot.Files.Take(500) });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { message = ex.Message });
    }
});

app.MapPost("/api/chat/stream", async (ChatRequest request, HttpResponse response, WebRuntime runtime, CancellationToken cancellationToken) =>
{
    response.Headers.ContentType = "text/event-stream";
    response.Headers.CacheControl = "no-cache";
    try
    {
        await runtime.StreamChatAsync(request, async text =>
        {
            await response.WriteAsync($"data: {System.Text.Json.JsonSerializer.Serialize(new { text })}\n\n", cancellationToken);
            await response.Body.FlushAsync(cancellationToken);
        }, cancellationToken);
        await response.WriteAsync("event: done\ndata: {}\n\n", cancellationToken);
    }
    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { }
    catch (Exception ex)
    {
        await response.WriteAsync($"event: error\ndata: {System.Text.Json.JsonSerializer.Serialize(new { message = SecretSanitizer.Sanitize(ex.Message) })}\n\n", CancellationToken.None);
    }
});

app.Run();

public sealed record ConnectRequest(string ApiKey, string? ProviderId, string? Endpoint);
public sealed record ProjectRequest(string Path);
public sealed record ChatRequest(string Prompt, string? ProviderId, string? Model, string ProjectPath, bool Cloud = true);

public sealed class WebRuntime
{
    private readonly WindowsCredentialManager _credentials = new();
    private readonly ProjectWorkspaceService _workspace = new();
    private readonly IConversationService _conversations;
    private readonly IReadOnlyList<IAIProvider> _directProviders;
    private readonly AutoDetectProvider _auto;
    private readonly IProviderManager _manager;
    private readonly WorkspaceChatService _chat;
    private readonly ConcurrentDictionary<string, IReadOnlyList<AIModel>> _models = new(StringComparer.OrdinalIgnoreCase);

    public WebRuntime()
    {
        _directProviders = new IAIProvider[]
        {
            new OpenAIProvider(_credentials, new HttpClient()),
            new GeminiProvider(_credentials),
            new AnthropicProvider(_credentials),
            new OpenAICompatibleProvider(_credentials)
        };
        _auto = new AutoDetectProvider(_credentials, _directProviders);
        var providers = new[] { (IAIProvider)_auto }.Concat(_directProviders).ToArray();
        _manager = new ProviderManager(_credentials, providers);
        var options = new DbContextOptionsBuilder<KeyGoDbContext>().UseInMemoryDatabase($"keygo-web-{Guid.NewGuid():N}").Options;
        _conversations = new ConversationService(new KeyGoDbContext(options));
        _chat = new WorkspaceChatService(new ProjectContextService(_workspace), providers, _conversations);
    }

    public object ProviderCards() => new[]
    {
        new { id = "auto", name = "Auto-detect provider", description = "Enter one key and KeyGo probes supported providers." },
        new { id = "openai", name = "OpenAI", description = "OpenAI API" },
        new { id = "gemini", name = "Google Gemini", description = "Google AI API" },
        new { id = "anthropic", name = "Anthropic", description = "Claude API" },
        new { id = "openai-compatible", name = "Custom endpoint", description = "OpenAI-compatible API" }
    };

    public IReadOnlyList<AIModel> Models(string? providerId) => providerId is not null && _models.TryGetValue(providerId, out var models) ? models : Array.Empty<AIModel>();

    public async Task<ProviderVerificationResult> ConnectAsync(string apiKey, string? providerId, string? endpoint, CancellationToken cancellationToken)
    {
        var id = string.IsNullOrWhiteSpace(providerId) ? "auto" : providerId;
        foreach (var provider in _directProviders)
        {
            await _credentials.SaveAsync(provider.Id, "default", apiKey, cancellationToken);
        }
        await _credentials.SaveAsync("auto", "default", apiKey, cancellationToken);
        if (id == "openai-compatible" && _directProviders.OfType<OpenAICompatibleProvider>().FirstOrDefault() is { } compatible && !string.IsNullOrWhiteSpace(endpoint)) compatible.BaseUrl = endpoint;
        var result = await _manager.VerifyProviderAsync(id, cancellationToken: cancellationToken);
        if (result.IsVerified) _models[id] = await _manager.DiscoverModelsAsync(id, cancellationToken: cancellationToken);
        return result;
    }

    public async Task<ProjectIndexSnapshot> OpenProjectAsync(string path, CancellationToken cancellationToken) => await _workspace.IndexAsync(path, cancellationToken);

    public async Task StreamChatAsync(ChatRequest request, Func<string, Task> onText, CancellationToken cancellationToken)
    {
        var providerId = string.IsNullOrWhiteSpace(request.ProviderId) ? "auto" : request.ProviderId;
        var model = request.Model;
        if (string.IsNullOrWhiteSpace(model) && _models.TryGetValue(providerId, out var models)) model = models.FirstOrDefault()?.Id;
        if (string.IsNullOrWhiteSpace(model)) throw new InvalidOperationException("Connect a provider and select a model first.");
        await _chat.SendAsync(request.ProjectPath, providerId, model, request.Prompt, request.Cloud, onText: text => onText(text), cancellationToken);
    }
}
