using KeyGo.Core.Abstractions;
using KeyGo.Core.Models;

namespace KeyGo.Core.Services;

public sealed class WorkspaceChatResult
{
    public required Conversation Conversation { get; init; }
    public required ProjectContextResult Context { get; init; }
    public required AIResponse Response { get; init; }
}

public sealed class WorkspaceChatService
{
    private readonly ProjectContextService _context;
    private readonly IReadOnlyDictionary<string, IAIProvider> _providers;
    private readonly IConversationService _conversations;

    public WorkspaceChatService(
        ProjectContextService context,
        IEnumerable<IAIProvider> providers,
        IConversationService conversations)
    {
        _context = context;
        _providers = providers.ToDictionary(provider => provider.Id, StringComparer.OrdinalIgnoreCase);
        _conversations = conversations;
    }

    public async Task<WorkspaceChatResult> SendAsync(
        string projectRoot,
        string providerId,
        string model,
        string prompt,
        bool cloudRequest = true,
        Action<string>? onText = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(prompt))
        {
            throw new ArgumentException("A prompt is required.", nameof(prompt));
        }

        if (!_providers.TryGetValue(providerId, out var provider))
        {
            throw new InvalidOperationException($"Provider '{providerId}' is not registered.");
        }

        var context = await _context.BuildAsync(projectRoot, prompt, cloudRequest, cancellationToken);
        var conversation = await _conversations.CreateConversationAsync(
            prompt.Length > 80 ? prompt[..80] : prompt,
            provider.Id,
            model,
            cancellationToken);

        await _conversations.AddMessageAsync(conversation.Id, "user", prompt, cancellationToken: cancellationToken);

        var request = new AIRequest
        {
            Provider = provider.Id,
            Model = model,
            Prompt = $"Project context:\n{context.Content}\n\nUser request:\n{prompt}",
            Metadata = new Dictionary<string, object>
            {
                ["projectRoot"] = projectRoot,
                ["contextFiles"] = context.IncludedFiles.Count,
                ["estimatedContextTokens"] = context.EstimatedTokens
            }
        };

        var content = new System.Text.StringBuilder();
        await foreach (var item in provider.StreamAsync(request, cancellationToken))
        {
            if (!string.Equals(item.Type, "text", StringComparison.OrdinalIgnoreCase) || string.IsNullOrEmpty(item.Value))
            {
                continue;
            }

            content.Append(item.Value);
            onText?.Invoke(item.Value);
        }

        var response = new AIResponse
        {
            Content = content.ToString(),
            Provider = provider.Id,
            Model = model,
            InputTokens = context.EstimatedTokens + Math.Max(1, prompt.Length / 4),
            OutputTokens = Math.Max(1, content.Length / 4),
            Status = "Estimated"
        };

        await _conversations.AddMessageAsync(conversation.Id, "assistant", response.Content, cancellationToken: cancellationToken);

        return new WorkspaceChatResult
        {
            Conversation = conversation,
            Context = context,
            Response = response
        };
    }
}