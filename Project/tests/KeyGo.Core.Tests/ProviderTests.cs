using KeyGo.Core.Abstractions;
using KeyGo.Core.Models;
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
    public async Task OpenAIProvider_ReportsNormalizedCapabilities()
    {
        var credentials = new WindowsCredentialManager();
        await credentials.SaveAsync("openai", "default", "sk-test");

        var provider = new OpenAIProvider(credentials, new HttpClient(new OpenAIModelsHandler()));
        var capabilities = await provider.GetCapabilitiesAsync();

        Assert.Contains(ProviderCapability.Text, capabilities.SupportedCapabilities);
        Assert.Contains(ProviderCapability.Streaming, capabilities.SupportedCapabilities);
        Assert.Contains(ProviderCapability.ToolCalling, capabilities.SupportedCapabilities);
        Assert.Contains(ProviderCapability.StructuredOutput, capabilities.SupportedCapabilities);
    }

    [Fact]
    public async Task ProviderCompatibilityService_CalculatesCompatibilityScore()
    {
        var provider = new OpenAIProvider(new WindowsCredentialManager(), new HttpClient(new OpenAIModelsHandler()));
        var service = new ProviderCompatibilityService();

        var result = await service.EvaluateAsync(provider);

        Assert.True(result.Score >= 70);
        Assert.Contains(result.VerifiedCapabilities, item => item == ProviderCapability.Text || item == ProviderCapability.Streaming);
        Assert.NotNull(result.Reason);
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

    private sealed class OpenAIModelsHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new
                {
                    data = new[]
                    {
                        new { id = "gpt-4o-mini", @object = "model", created = 0, owned_by = "openai" },
                        new { id = "gpt-4o", @object = "model", created = 0, owned_by = "openai" }
                    }
                })
            };
            return Task.FromResult(response);
        }
    }

    [Fact]
    public void SecretSanitizer_RedactsSensitiveValues()
    {
        var raw = "Authorization: Bearer sk-live-12345; password=super-secret; api_key=secret-token";

        var sanitized = SecretSanitizer.Sanitize(raw);

        Assert.DoesNotContain("sk-live-12345", sanitized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("super-secret", sanitized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("secret-token", sanitized, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("[REDACTED]", sanitized, StringComparison.OrdinalIgnoreCase);
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
