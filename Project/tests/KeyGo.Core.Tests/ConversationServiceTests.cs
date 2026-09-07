using KeyGo.Core.Models;
using KeyGo.Core.Services;
using KeyGo.Core.Storage;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace KeyGo.Core.Tests;

public class ConversationServiceTests
{
    [Fact]
    public async Task CreateConversationAsync_CreatesConversationAndPersistsIt()
    {
        var options = new DbContextOptionsBuilder<KeyGoDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var dbContext = new KeyGoDbContext(options);
        var service = new ConversationService(dbContext);

        var conversation = await service.CreateConversationAsync("Test chat", "OpenAI", "gpt-4o-mini");
        var loaded = await service.GetConversationAsync(conversation.Id);

        Assert.NotNull(loaded);
        Assert.Equal("Test chat", loaded!.Title);
        Assert.Equal("OpenAI", loaded.Provider);
    }

    [Fact]
    public async Task AddMessageAsync_AddsMessageToConversation()
    {
        var options = new DbContextOptionsBuilder<KeyGoDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var dbContext = new KeyGoDbContext(options);
        var service = new ConversationService(dbContext);

        var conversation = await service.CreateConversationAsync("Chat", "OpenAI", "gpt-4o-mini");
        await service.AddMessageAsync(conversation.Id, "user", "hello");

        var loaded = await service.GetConversationAsync(conversation.Id);
        Assert.NotNull(loaded);
        Assert.Contains(loaded!.Messages, m => m.Content == "hello");
    }
}
