namespace KeyGo.Core.Models;

public enum ProviderCapability
{
    Text,
    Streaming,
    Vision,
    ImageGeneration,
    AudioInput,
    AudioOutput,
    ToolCalling,
    StructuredOutput,
    Embeddings,
    Reasoning,
    LongContext
}

public sealed class ProviderCapabilities
{
    public string Provider { get; init; } = string.Empty;
    public IReadOnlyList<ProviderCapability> SupportedCapabilities { get; init; } = Array.Empty<ProviderCapability>();
    public IReadOnlyList<ProviderCapability> UnsupportedCapabilities { get; init; } = Array.Empty<ProviderCapability>();
    public bool IsSupported(ProviderCapability capability) => SupportedCapabilities.Contains(capability);

    public static ProviderCapabilities Create(string provider, IEnumerable<ProviderCapability>? supported = null, IEnumerable<ProviderCapability>? unsupported = null)
    {
        return new ProviderCapabilities
        {
            Provider = provider,
            SupportedCapabilities = (supported ?? Array.Empty<ProviderCapability>()).ToArray(),
            UnsupportedCapabilities = (unsupported ?? Array.Empty<ProviderCapability>()).ToArray()
        };
    }
}

public sealed class AIModel
{
    public required string Id { get; init; }
    public string? DisplayName { get; init; }
    public string? Provider { get; init; }
    public int? ContextLength { get; init; }
    public bool? SupportsVision { get; init; }
    public bool? SupportsAudio { get; init; }
    public bool? SupportsTools { get; init; }
    public bool? SupportsImageGeneration { get; init; }
    public bool? SupportsReasoning { get; init; }
    public decimal? InputPricePer1M { get; init; }
    public decimal? OutputPricePer1M { get; init; }
    public string Status { get; init; } = "Unknown";
}

public sealed class ProviderCompatibilityResult
{
    public required string Provider { get; init; }
    public int Score { get; init; }
    public IReadOnlyList<ProviderCapability> VerifiedCapabilities { get; init; } = Array.Empty<ProviderCapability>();
    public string Reason { get; init; } = string.Empty;
    public bool IsCompatible => Score >= 70;
}

public static class SecretSanitizer
{
    private static readonly string[] SensitivePatterns =
    [
        "authorization",
        "api[_-]?key",
        "apikey",
        "secret",
        "password",
        "token",
        "bearer "
    ];

    public static string Sanitize(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;

        var text = value;
        foreach (var pattern in SensitivePatterns)
        {
            text = System.Text.RegularExpressions.Regex.Replace(
                text,
                $"(?i)({pattern})\\s*[:=]\\s*([^\\s,;\"']+)",
                "$1=[REDACTED]");
        }

        text = System.Text.RegularExpressions.Regex.Replace(text, "(?i)(Bearer\\s+)[A-Za-z0-9._-]+", "$1[REDACTED]");
        text = System.Text.RegularExpressions.Regex.Replace(text, "(?i)(sk-[A-Za-z0-9_-]{4,})", "[REDACTED]");
        text = System.Text.RegularExpressions.Regex.Replace(text, "(?i)(password=)[^\\s,;]+", "$1[REDACTED]");
        text = System.Text.RegularExpressions.Regex.Replace(text, "(?i)(api_key=)[^\\s,;]+", "$1[REDACTED]");
        return text;
    }
}
