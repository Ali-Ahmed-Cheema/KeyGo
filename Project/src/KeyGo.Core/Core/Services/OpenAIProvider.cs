using KeyGo.Core.Abstractions;
using KeyGo.Core.Models;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace KeyGo.Core.Services;

public sealed class OpenAIProvider : IAIProvider
{
    private readonly ICredentialManager _credentialManager;
    private readonly HttpClient _httpClient;

    public OpenAIProvider(ICredentialManager credentialManager, HttpClient httpClient)
    {
        _credentialManager = credentialManager;
        _httpClient = httpClient;
    }

    public string Id => "openai";
    public string DisplayName => "OpenAI";

    public async Task<ProviderConnectionResult> ValidateAsync(CancellationToken cancellationToken = default)
    {
        var apiKey = await GetApiKeyAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return ProviderConnectionResult.Failed(Id, "No API key configured.");
        }

        using var request = CreateRequest(HttpMethod.Get, "models", apiKey);
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return ProviderConnectionResult.Failed(Id, $"OpenAI rejected the credential ({(int)response.StatusCode}).");
        }

        return ProviderConnectionResult.Connected(Id, "OpenAI is reachable and the credential is valid.", new[] { "Authentication", "Model discovery", "Streaming" });
    }

    public async Task<IReadOnlyList<AIModel>> GetModelsAsync(CancellationToken cancellationToken = default)
    {
        var apiKey = await GetApiKeyAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(apiKey)) return Array.Empty<AIModel>();

        using var request = CreateRequest(HttpMethod.Get, "models", apiKey);
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        await EnsureSuccessAsync(response);
        using var document = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);

        return document.RootElement.GetProperty("data")
            .EnumerateArray()
            .Select(model => model.GetProperty("id").GetString())
            .Where(id => !string.IsNullOrWhiteSpace(id) && id.StartsWith("gpt-", StringComparison.OrdinalIgnoreCase))
            .Select(id => new AIModel
            {
                Id = id!,
                DisplayName = id,
                Provider = Id,
                Status = "Discovered"
            })
            .OrderBy(model => model.Id)
            .ToArray();
    }

    public async Task<AIResponse> SendAsync(AIRequest request, CancellationToken cancellationToken = default)
    {
        var apiKey = await GetApiKeyAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(apiKey)) throw new InvalidOperationException("No OpenAI API key configured.");

        using var httpRequest = CreateRequest(HttpMethod.Post, "chat/completions", apiKey);
        httpRequest.Content = JsonContent.Create(new
        {
            model = request.Model ?? "gpt-4o-mini",
            messages = new[] { new { role = "user", content = request.Prompt } }
        });

        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        await EnsureSuccessAsync(response);
        using var document = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);
        var root = document.RootElement;
        var usage = root.TryGetProperty("usage", out var usageElement) ? usageElement : default;
        return new AIResponse
        {
            Content = root.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString() ?? string.Empty,
            Provider = Id,
            Model = request.Model ?? "gpt-4o-mini",
            InputTokens = usage.ValueKind == JsonValueKind.Undefined ? 0 : usage.GetProperty("prompt_tokens").GetInt64(),
            OutputTokens = usage.ValueKind == JsonValueKind.Undefined ? 0 : usage.GetProperty("completion_tokens").GetInt64(),
            Status = usage.ValueKind == JsonValueKind.Undefined ? "Unreported" : "ProviderReported"
        };
    }

    public async IAsyncEnumerable<AIStreamEvent> StreamAsync(AIRequest request, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var apiKey = await GetApiKeyAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(apiKey)) throw new InvalidOperationException("No OpenAI API key configured.");

        using var httpRequest = CreateRequest(HttpMethod.Post, "chat/completions", apiKey);
        httpRequest.Content = JsonContent.Create(new
        {
            model = request.Model ?? "gpt-4o-mini",
            stream = true,
            messages = new[] { new { role = "user", content = request.Prompt } }
        });

        using var response = await _httpClient.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        await EnsureSuccessAsync(response);
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream, Encoding.UTF8);
        while (await reader.ReadLineAsync(cancellationToken) is { } line)
        {
            if (!line.StartsWith("data:", StringComparison.OrdinalIgnoreCase)) continue;
            var data = line[5..].Trim();
            if (data == "[DONE]")
            {
                yield return new AIStreamEvent { Type = "text", IsFinal = true };
                yield break;
            }

            using var document = JsonDocument.Parse(data);
            var delta = document.RootElement.GetProperty("choices")[0].GetProperty("delta");
            if (delta.TryGetProperty("content", out var content) && content.ValueKind == JsonValueKind.String)
            {
                yield return new AIStreamEvent { Type = "text", Value = content.GetString() ?? string.Empty };
            }
        }

        yield return new AIStreamEvent { Type = "text", IsFinal = true };
    }

    private async Task<string?> GetApiKeyAsync(CancellationToken cancellationToken)
    {
        return await _credentialManager.GetAsync(Id, "default", cancellationToken);
    }

    private HttpRequestMessage CreateRequest(HttpMethod method, string path, string apiKey)
    {
        var request = new HttpRequestMessage(method, $"https://api.openai.com/v1/{path}");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
        return request;
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode) return;
        throw new HttpRequestException($"OpenAI request failed with status {(int)response.StatusCode}.");
    }
}
