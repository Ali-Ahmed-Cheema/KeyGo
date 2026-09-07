using KeyGo.Core.Services;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace KeyGo.Core.Tests;

public class ProviderManagerTests
{
    [Fact]
    public async Task VerifyProviderAsync_ReturnsConnected_WhenCredentialExists()
    {
        var credentialManager = new WindowsCredentialManager();
        await credentialManager.SaveAsync("openai", "default", "sk-valid");

        var provider = new OpenAIProvider(credentialManager, CreateHttpClient());
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

        var provider = new OpenAIProvider(credentialManager, CreateHttpClient());
        var manager = new ProviderManager(credentialManager, new[] { provider });

        var models = await manager.DiscoverModelsAsync("openai");

        Assert.NotEmpty(models);
        Assert.Contains(models, m => m.Id == "gpt-4o-mini");
    }

    private static HttpClient CreateHttpClient()
    {
        return new HttpClient(new StubOpenAiHandler());
    }

    private sealed class StubOpenAiHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new
                {
                    data = new[] { new { id = "gpt-4o-mini" }, new { id = "gpt-4o" } }
                })
            };

            return Task.FromResult(response);
        }
    }
}
