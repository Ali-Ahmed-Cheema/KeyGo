using KeyGo.Core.Services;
using Xunit;

namespace KeyGo.Core.Tests;

public class ProviderManagerTests
{
    [Fact]
    public async Task VerifyProviderAsync_ReturnsConnected_WhenCredentialExists()
    {
        var credentialManager = new WindowsCredentialManager();
        await credentialManager.SaveAsync("openai", "default", "sk-valid");

        var provider = new OpenAIProvider(credentialManager, new HttpClient());
        var manager = new ProviderManager(credentialManager, new[] { provider });

        var result = await manager.VerifyProviderAsync("openai");

        Assert.True(result.IsVerified);
        Assert.Equal("openai", result.ProviderId);
        Assert.NotEmpty(result.VerifiedCapabilities);
    }

    [Fact]
    public async Task DiscoverModelsAsync_ReturnsModels_WhenProviderConfigured()
    {
        var credentialManager = new WindowsCredentialManager();
        await credentialManager.SaveAsync("openai", "default", "sk-valid");

        var provider = new OpenAIProvider(credentialManager, new HttpClient());
        var manager = new ProviderManager(credentialManager, new[] { provider });

        var models = await manager.DiscoverModelsAsync("openai");

        Assert.NotEmpty(models);
        Assert.Contains(models, m => m.Id == "gpt-4o-mini");
    }
}
