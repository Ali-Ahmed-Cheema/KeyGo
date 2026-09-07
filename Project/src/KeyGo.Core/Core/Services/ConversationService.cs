using KeyGo.Core.Models;
using KeyGo.Core.Storage;
using Microsoft.EntityFrameworkCore;

namespace KeyGo.Core.Services;

public interface IConversationService
{
    Task<Conversation> CreateConversationAsync(string title, string provider, string model, CancellationToken cancellationToken = default);
    Task<List<Conversation>> GetConversationsAsync(CancellationToken cancellationToken = default);
    Task<Conversation?> GetConversationAsync(Guid conversationId, CancellationToken cancellationToken = default);
    Task AddMessageAsync(Guid conversationId, string role, string content, string messageType = "Text", CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

public sealed class ConversationService : IConversationService
{
    private readonly KeyGoDbContext _dbContext;

    public ConversationService(KeyGoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Conversation> CreateConversationAsync(string title, string provider, string model, CancellationToken cancellationToken = default)
    {
        var conversation = new Conversation
        {
            Title = string.IsNullOrWhiteSpace(title) ? "New Conversation" : title,
            Provider = provider,
            Model = model,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        _dbContext.Conversations.Add(conversation);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return conversation;
    }

    public async Task<List<Conversation>> GetConversationsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Conversations
            .Include(c => c.Messages)
            .OrderByDescending(c => c.UpdatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<Conversation?> GetConversationAsync(Guid conversationId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Conversations
            .Include(c => c.Messages)
            .FirstOrDefaultAsync(c => c.Id == conversationId, cancellationToken);
    }

    public async Task AddMessageAsync(Guid conversationId, string role, string content, string messageType = "Text", CancellationToken cancellationToken = default)
    {
        var conversation = await _dbContext.Conversations
            .FirstOrDefaultAsync(c => c.Id == conversationId, cancellationToken);

        if (conversation is null)
        {
            throw new InvalidOperationException($"Conversation '{conversationId}' not found.");
        }

        var message = new ChatMessage
        {
            ConversationId = conversationId,
            Role = role,
            Content = content,
            MessageType = messageType,
            CreatedAtUtc = DateTime.UtcNow
        };

        _dbContext.ChatMessages.Add(message);
        conversation.UpdatedAtUtc = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
