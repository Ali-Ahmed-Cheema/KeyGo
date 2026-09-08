using KeyGo.Core.Abstractions;
using KeyGo.Core.Models;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace KeyGo.Core.Services;

public sealed class OpenAICompatibleProvider : IAIProvider
{
    private readonly ICredentialManager _credentialManager;
    private readonly HttpClient _httpClient;
    private string _baseUrl;

    public OpenAICompatibleProvider(ICredentialManager credentialManager, HttpClient? httpClient = null, string? baseUrl = null)
    {
        _credentialManager = credentialManager;
        _httpClient = httpClient ?? new HttpClient();
        _baseUrl = NormalizeBaseUrl(baseUrl ?? "https://api.openai.com/v1");
    }

    public string Id => "openai-compatible";
    public string DisplayName => "OpenAI-compatible";
    public string BaseUrl { get => _baseUrl; set => _baseUrl = NormalizeBaseUrl(value); }

    public async Task<ProviderConnectionResult> ValidateAsync(CancellationToken cancellationToken = default)
    {
        var apiKey = await GetApiKeyAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(apiKey)) return ProviderConnectionResult.Failed(Id, "No API key configured.");

        using var response = await SendAsync(HttpMethod.Get, "models", apiKey, cancellationToken);
        if (!response.IsSuccessStatusCode) return ProviderConnectionResult.Failed(Id, $"The endpoint rejected the credential ({(int)response.StatusCode}).");
        return ProviderConnectionResult.Connected(Id, "Endpoint is reachable and the credential is valid.", new[] { "Authentication", "Model discovery", "Streaming" });
    }

    public Task<ProviderCapabilities> GetCapabilitiesAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(ProviderCapabilities.Create(Id,
            supported: new[] { ProviderCapability.Text, ProviderCapability.Streaming, ProviderCapability.ToolCalling, ProviderCapability.StructuredOutput },
            unsupported: new[] { ProviderCapability.ImageGeneration, ProviderCapability.AudioInput, ProviderCapability.AudioOutput }));
    }

    public async Task<IReadOnlyList<AIModel>> GetModelsAsync(CancellationToken cancellationToken = default)
    {
        var apiKey = await GetApiKeyAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(apiKey)) return Array.Empty<AIModel>();

        using var response = await SendAsync(HttpMethod.Get, "models", apiKey, cancellationToken);
        await EnsureSuccessAsync(response);
        using var document = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);
        if (!document.RootElement.TryGetProperty("data", out var data) || data.ValueKind != JsonValueKind.Array) return Array.Empty<AIModel>();

        return data.EnumerateArray()
            .Where(model => model.TryGetProperty("id", out var id) && !string.IsNullOrWhiteSpace(id.GetString()))
            .Select(model => model.GetProperty("id").GetString()!)
            .Select(id => new AIModel { Id = id, DisplayName = id, Provider = Id, Status = "Discovered" })
            .OrderBy(model => model.Id)
            .ToArray();
    }

    public async Task<AIResponse> SendAsync(AIRequest request, CancellationToken cancellationToken = default)
    {
        var apiKey = await GetApiKeyAsync(cancellationToken) ?? throw new InvalidOperationException("No API key configured.");
        using var httpRequest = CreateRequest(HttpMethod.Post, "chat/completions", apiKey);
        httpRequest.Content = JsonContent.Create(new { model = request.Model ?? "default", messages = new[] { new { role = "user", content = request.Prompt } } });
        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        await EnsureSuccessAsync(response);
        using var document = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);
        var root = document.RootElement;
        return new AIResponse { Content = ReadContent(root), Provider = Id, Model = request.Model ?? "default", Status = "ProviderReported" };
    }

    public async IAsyncEnumerable<AIStreamEvent> StreamAsync(AIRequest request, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var apiKey = await GetApiKeyAsync(cancellationToken) ?? throw new InvalidOperationException("No API key configured.");
        using var httpRequest = CreateRequest(HttpMethod.Post, "chat/completions", apiKey);
        httpRequest.Content = JsonContent.Create(new { model = request.Model ?? "default", stream = true, messages = new[] { new { role = "user", content = request.Prompt } } });
        using var response = await _httpClient.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        await EnsureSuccessAsync(response);
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream, Encoding.UTF8);
        while (await reader.ReadLineAsync(cancellationToken) is { } line)
        {
            if (!line.StartsWith("data:", StringComparison.OrdinalIgnoreCase)) continue;
            var data = line[5..].Trim();
            if (data == "[DONE]") { yield return new AIStreamEvent { Type = "text", IsFinal = true }; yield break; }
            using var document = JsonDocument.Parse(data);
            var delta = document.RootElement.GetProperty("choices")[0].GetProperty("delta");
            if (delta.TryGetProperty("content", out var content) && content.ValueKind == JsonValueKind.String)
                yield return new AIStreamEvent { Type = "text", Value = content.GetString() ?? string.Empty };
        }
        yield return new AIStreamEvent { Type = "text", IsFinal = true };
    }

    private async Task<string?> GetApiKeyAsync(CancellationToken cancellationToken) => await _credentialManager.GetAsync(Id, "default", cancellationToken);

    private async Task<HttpResponseMessage> SendAsync(HttpMethod method, string path, string apiKey, CancellationToken cancellationToken)
        => await _httpClient.SendAsync(CreateRequest(method, path, apiKey), cancellationToken);

    private HttpRequestMessage CreateRequest(HttpMethod method, string path, string apiKey)
    {
        var request = new HttpRequestMessage(method, $"{_baseUrl}/{path}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        return request;
    }

    private static string ReadContent(JsonElement root) => root.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0
        ? choices[0].GetProperty("message").GetProperty("content").GetString() ?? string.Empty : string.Empty;

    private static string NormalizeBaseUrl(string value) => value.Trim().TrimEnd('/');

    private static async Task EnsureSuccessAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode) return;
        var details = await response.Content.ReadAsStringAsync();
        throw new HttpRequestException($"Compatible provider request failed with status {(int)response.StatusCode}: {SecretSanitizer.Sanitize(details)}");
    }
}
