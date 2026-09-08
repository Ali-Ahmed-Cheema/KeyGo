using KeyGo.Core.Abstractions;
using KeyGo.Core.Models;
using System.Net.Http.Json;
using System.Text.Json;

namespace KeyGo.Core.Services;

public sealed class GeminiProvider : IAIProvider
{
    private readonly ICredentialManager _credentialManager;
    private readonly HttpClient _httpClient;

    public GeminiProvider(ICredentialManager credentialManager, HttpClient? httpClient = null)
    {
        _credentialManager = credentialManager;
        _httpClient = httpClient ?? new HttpClient();
    }

    public string Id => "gemini";
    public string DisplayName => "Google Gemini";

    public async Task<ProviderConnectionResult> ValidateAsync(CancellationToken cancellationToken = default)
    {
        var apiKey = await _credentialManager.GetAsync(Id, "default", cancellationToken);
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return ProviderConnectionResult.Failed(Id, "No Gemini API key configured.");
        }

        using var response = await _httpClient.GetAsync($"https://generativelanguage.googleapis.com/v1beta/models?key={Uri.EscapeDataString(apiKey)}", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return ProviderConnectionResult.Failed(Id, $"Gemini rejected the credential ({(int)response.StatusCode}).");
        }

        return ProviderConnectionResult.Connected(Id, "Gemini is reachable and the credential is valid.", new[] { "Authentication", "Model discovery", "Streaming" }, new[] { "Vision", "Audio", "Tool calling" });
    }

    public async Task<IReadOnlyList<AIModel>> GetModelsAsync(CancellationToken cancellationToken = default)
    {
        var apiKey = await GetApiKeyAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(apiKey)) return Array.Empty<AIModel>();

        using var response = await _httpClient.GetAsync($"https://generativelanguage.googleapis.com/v1beta/models?key={Uri.EscapeDataString(apiKey)}", cancellationToken);
        await EnsureSuccessAsync(response);
        using var document = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);

        return document.RootElement.GetProperty("models")
            .EnumerateArray()
            .Where(model => model.TryGetProperty("supportedGenerationMethods", out var methods)
                && methods.EnumerateArray().Any(method => string.Equals(method.GetString(), "generateContent", StringComparison.OrdinalIgnoreCase)))
            .Select(model => new AIModel
            {
                Id = model.GetProperty("name").GetString()!.Replace("models/", string.Empty, StringComparison.OrdinalIgnoreCase),
                DisplayName = model.TryGetProperty("displayName", out var name) ? name.GetString() : null,
                Provider = Id,
                ContextLength = model.TryGetProperty("inputTokenLimit", out var context) ? context.GetInt32() : null,
                SupportsVision = true,
                SupportsTools = true,
                SupportsReasoning = true,
                Status = "Discovered"
            })
            .OrderBy(model => model.Id)
            .ToArray();
    }

    public async Task<AIResponse> SendAsync(AIRequest request, CancellationToken cancellationToken = default)
    {
        var apiKey = await GetApiKeyAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(apiKey)) throw new InvalidOperationException("No Gemini API key configured.");
        var model = request.Model ?? "gemini-1.5-flash";
        using var response = await _httpClient.PostAsJsonAsync(BuildUrl(model, apiKey, streaming: false), CreatePayload(request), cancellationToken);
        await EnsureSuccessAsync(response);
        using var document = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);
        var root = document.RootElement;
        var content = ReadText(root);
        return new AIResponse
        {
            Content = content,
            Provider = Id,
            Model = model,
            InputTokens = ReadUsage(root, "promptTokenCount"),
            OutputTokens = ReadUsage(root, "candidatesTokenCount"),
            Status = "ProviderReported"
        };
    }

    public async IAsyncEnumerable<AIStreamEvent> StreamAsync(AIRequest request, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var apiKey = await GetApiKeyAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(apiKey)) throw new InvalidOperationException("No Gemini API key configured.");
        var model = request.Model ?? "gemini-1.5-flash";
        using var response = await _httpClient.PostAsJsonAsync(BuildUrl(model, apiKey, streaming: true), CreatePayload(request), cancellationToken);
        await EnsureSuccessAsync(response);
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);
        while (await reader.ReadLineAsync(cancellationToken) is { } line)
        {
            if (!line.StartsWith("data:", StringComparison.OrdinalIgnoreCase)) continue;
            var json = line[5..].Trim();
            if (string.IsNullOrWhiteSpace(json)) continue;
            using var document = JsonDocument.Parse(json);
            var text = ReadText(document.RootElement);
            if (!string.IsNullOrEmpty(text)) yield return new AIStreamEvent { Type = "text", Value = text };
        }

        yield return new AIStreamEvent { Type = "text", Value = string.Empty, IsFinal = true };
    }

    private async Task<string?> GetApiKeyAsync(CancellationToken cancellationToken) => await _credentialManager.GetAsync(Id, "default", cancellationToken);

    private static string BuildUrl(string model, string apiKey, bool streaming) =>
        $"https://generativelanguage.googleapis.com/v1beta/models/{Uri.EscapeDataString(model)}:{(streaming ? "streamGenerateContent?alt=sse&key=" : "generateContent?key=")}{Uri.EscapeDataString(apiKey)}";

    private static object CreatePayload(AIRequest request) => new
    {
        contents = new[] { new { parts = new[] { new { text = request.Prompt } } } }
    };

    private static string ReadText(JsonElement root)
    {
        if (!root.TryGetProperty("candidates", out var candidates) || candidates.GetArrayLength() == 0) return string.Empty;
        var parts = candidates[0].GetProperty("content").GetProperty("parts");
        return string.Concat(parts.EnumerateArray().Where(part => part.TryGetProperty("text", out _)).Select(part => part.GetProperty("text").GetString()));
    }

    private static long ReadUsage(JsonElement root, string property)
    {
        return root.TryGetProperty("usageMetadata", out var usage) && usage.TryGetProperty(property, out var value) ? value.GetInt64() : 0;
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode) return;
        var details = await response.Content.ReadAsStringAsync();
        throw new HttpRequestException($"Gemini request failed with status {(int)response.StatusCode}: {ExtractError(details)}");
    }

    private static string ExtractError(string details)
    {
        try
        {
            using var document = JsonDocument.Parse(details);
            if (document.RootElement.TryGetProperty("error", out var error)
                && error.TryGetProperty("message", out var message))
            {
                return message.GetString() ?? "The provider rejected the request.";
            }
        }
        catch (JsonException)
        {
            // Keep a generic message when the provider response is not JSON.
        }

        return "The provider rejected the request.";
    }
}
