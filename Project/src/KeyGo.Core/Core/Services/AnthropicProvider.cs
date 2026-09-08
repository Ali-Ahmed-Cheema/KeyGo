using KeyGo.Core.Abstractions;
using KeyGo.Core.Models;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace KeyGo.Core.Services;

public sealed class AnthropicProvider : IAIProvider
{
    private readonly ICredentialManager _credentialManager;
    private readonly HttpClient _httpClient;

    public AnthropicProvider(ICredentialManager credentialManager, HttpClient? httpClient = null)
    {
        _credentialManager = credentialManager;
        _httpClient = httpClient ?? new HttpClient();
    }

    public string Id => "anthropic";
    public string DisplayName => "Anthropic";

    public async Task<ProviderConnectionResult> ValidateAsync(CancellationToken cancellationToken = default)
    {
        var apiKey = await _credentialManager.GetAsync(Id, "default", cancellationToken);
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return ProviderConnectionResult.Failed(Id, "No Anthropic API key configured.");
        }

        using var response = await SendHttpAsync(HttpMethod.Get, "models", apiKey, cancellationToken);
        return response.IsSuccessStatusCode
            ? ProviderConnectionResult.Connected(Id, "Anthropic is reachable and the credential is valid.", new[] { "Authentication", "Model discovery", "Streaming" }, new[] { "Vision", "Audio" })
            : ProviderConnectionResult.Failed(Id, $"Anthropic rejected the credential ({(int)response.StatusCode}).");
    }

    public Task<ProviderCapabilities> GetCapabilitiesAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(ProviderCapabilities.Create(
            Id,
            supported: new[]
            {
                ProviderCapability.Text,
                ProviderCapability.Streaming,
                ProviderCapability.Vision,
                ProviderCapability.ToolCalling,
                ProviderCapability.StructuredOutput,
                ProviderCapability.Reasoning,
                ProviderCapability.LongContext
            },
            unsupported: new[]
            {
                ProviderCapability.ImageGeneration,
                ProviderCapability.AudioInput,
                ProviderCapability.AudioOutput,
                ProviderCapability.Embeddings
            }));
    }

    public async Task<IReadOnlyList<AIModel>> GetModelsAsync(CancellationToken cancellationToken = default)
    {
        var apiKey = await GetApiKeyAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(apiKey)) return Array.Empty<AIModel>();
        using var response = await SendHttpAsync(HttpMethod.Get, "models", apiKey, cancellationToken);
        await EnsureSuccessAsync(response);
        using var document = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);
        return document.RootElement.GetProperty("data").EnumerateArray()
            .Select(model => new AIModel
            {
                Id = model.GetProperty("id").GetString()!,
                DisplayName = model.TryGetProperty("display_name", out var name) ? name.GetString() : null,
                Provider = Id,
                ContextLength = model.TryGetProperty("max_tokens", out var context) ? context.GetInt32() : null,
                SupportsVision = true,
                SupportsTools = true,
                SupportsReasoning = true,
                Status = "Discovered"
            })
            .ToArray();
    }

    public async Task<AIResponse> SendAsync(AIRequest request, CancellationToken cancellationToken = default)
    {
        var apiKey = await GetApiKeyAsync(cancellationToken) ?? throw new InvalidOperationException("No Anthropic API key configured.");
        var model = request.Model ?? "claude-3-5-sonnet-latest";
        using var httpRequest = CreateRequest(HttpMethod.Post, "messages", apiKey);
        httpRequest.Content = JsonContent.Create(new { model, max_tokens = 4096, messages = new[] { new { role = "user", content = request.Prompt } } });
        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        await EnsureSuccessAsync(response);
        using var document = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);
        return new AIResponse
        {
            Content = ReadText(document.RootElement),
            Provider = Id,
            Model = model,
            InputTokens = ReadUsage(document.RootElement, "input_tokens"),
            OutputTokens = ReadUsage(document.RootElement, "output_tokens"),
            Status = "ProviderReported"
        };
    }

    public async IAsyncEnumerable<AIStreamEvent> StreamAsync(AIRequest request, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var apiKey = await GetApiKeyAsync(cancellationToken) ?? throw new InvalidOperationException("No Anthropic API key configured.");
        var model = request.Model ?? "claude-3-5-sonnet-latest";
        using var httpRequest = CreateRequest(HttpMethod.Post, "messages", apiKey);
        httpRequest.Content = JsonContent.Create(new { model, max_tokens = 4096, stream = true, messages = new[] { new { role = "user", content = request.Prompt } } });
        using var response = await _httpClient.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        await EnsureSuccessAsync(response);
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream, Encoding.UTF8);
        while (await reader.ReadLineAsync(cancellationToken) is { } line)
        {
            if (!line.StartsWith("data:", StringComparison.OrdinalIgnoreCase)) continue;
            var data = line[5..].Trim();
            if (data == "[DONE]") break;
            using var document = JsonDocument.Parse(data);
            if (document.RootElement.TryGetProperty("delta", out var delta) && delta.TryGetProperty("text", out var text))
                yield return new AIStreamEvent { Type = "text", Value = text.GetString() ?? string.Empty };
        }
        yield return new AIStreamEvent { Type = "text", Value = string.Empty, IsFinal = true };
    }

    private async Task<string?> GetApiKeyAsync(CancellationToken cancellationToken) => await _credentialManager.GetAsync(Id, "default", cancellationToken);

    private HttpRequestMessage CreateRequest(HttpMethod method, string path, string apiKey)
    {
        var request = new HttpRequestMessage(method, $"https://api.anthropic.com/v1/{path}");
        request.Headers.Add("x-api-key", apiKey);
        request.Headers.Add("anthropic-version", "2023-06-01");
        return request;
    }

    private async Task<HttpResponseMessage> SendHttpAsync(HttpMethod method, string path, string apiKey, CancellationToken cancellationToken)
        => await _httpClient.SendAsync(CreateRequest(method, path, apiKey), cancellationToken);

    private static string ReadText(JsonElement root) => root.TryGetProperty("content", out var content)
        ? string.Concat(content.EnumerateArray().Where(item => item.TryGetProperty("text", out _)).Select(item => item.GetProperty("text").GetString()))
        : string.Empty;

    private static long ReadUsage(JsonElement root, string property) => root.TryGetProperty("usage", out var usage) && usage.TryGetProperty(property, out var value) ? value.GetInt64() : 0;

    private static async Task EnsureSuccessAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode) return;
        var details = await response.Content.ReadAsStringAsync();
        throw new HttpRequestException($"Anthropic request failed with status {(int)response.StatusCode}: {SecretSanitizer.Sanitize(details)}");
    }
}
