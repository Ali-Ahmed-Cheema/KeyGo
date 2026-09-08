using KeyGo.Core.Abstractions;
using KeyGo.Core.Services;
using System.Net;
using System.Net.Http.Json;
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

        var credentials = new WindowsCredentialManager();
        await credentials.SaveAsync("gemini", "default", "abc123");
        var provider = new GeminiProvider(credentials, new HttpClient(new GeminiHandler()));
        var models = await provider.GetModelsAsync();

        Assert.NotEmpty(models);
        Assert.Equal("gemini-1.5-flash", models[0].Id);
    }

    private sealed class GeminiHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new
                {
                    models = new[]
                    {
                        new { name = "models/gemini-1.5-flash", displayName = "Gemini 1.5 Flash", inputTokenLimit = 1048576, supportedGenerationMethods = new[] { "generateContent" } }
                    }
                })
            };
            return Task.FromResult(response);
        }
    }
}
