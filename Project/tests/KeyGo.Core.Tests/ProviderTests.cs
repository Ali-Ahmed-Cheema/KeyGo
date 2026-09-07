using KeyGo.Core.Abstractions;
using KeyGo.Core.Services;
using Xunit;

namespace KeyGo.Core.Tests;

public class ProviderTests
{
    [Fact]
    public async Task OpenAIProvider_ValidatesConfiguredCredentials()
    {
        var credentialStore = new InMemoryCredentialStore();
        await credentialStore.StoreAsync("openai", "default", "sk-test");

        var provider = new OpenAIProvider(new WindowsCredentialManager(), new HttpClient());
        await provider.ValidateAsync();

        Assert.Equal("openai", provider.Id);
    }

    [Fact]
    public async Task GeminiProvider_ReturnsModelList()
    {
        var credentialStore = new InMemoryCredentialStore();
        await credentialStore.StoreAsync("gemini", "default", "abc123");

        var provider = new GeminiProvider(new WindowsCredentialManager());
        var models = await provider.GetModelsAsync();

        Assert.NotEmpty(models);
        Assert.Equal("gemini-1.5-flash", models[0].Id);
    }
}
