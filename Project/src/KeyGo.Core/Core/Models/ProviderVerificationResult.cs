namespace KeyGo.Core.Models;

public enum ProviderVerificationStatus
{
    Connected,
    InvalidCredential,
    RateLimited,
    InsufficientCredits,
    NetworkError,
    ProviderError,
    Unknown,
    Disconnected
}

public sealed class ProviderVerificationResult
{
    public string ProviderId { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public ProviderVerificationStatus Status { get; init; }
    public bool IsVerified { get; init; }
    public string Message { get; init; } = string.Empty;
    public int ModelsDiscovered { get; init; }
    public IReadOnlyList<string> VerifiedCapabilities { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> UnknownCapabilities { get; init; } = Array.Empty<string>();
    public DateTime VerifiedAtUtc { get; init; } = DateTime.UtcNow;
}
