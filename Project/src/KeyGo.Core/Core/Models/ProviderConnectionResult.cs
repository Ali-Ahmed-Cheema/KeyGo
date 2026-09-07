namespace KeyGo.Core.Models;

public sealed class ProviderConnectionResult
{
    public bool IsConnected { get; init; }
    public string Provider { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public IReadOnlyList<string> VerifiedCapabilities { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> UnknownCapabilities { get; init; } = Array.Empty<string>();

    public static ProviderConnectionResult Connected(
        string provider,
        string message,
        IReadOnlyList<string>? verifiedCapabilities = null,
        IReadOnlyList<string>? unknownCapabilities = null)
    {
        return new ProviderConnectionResult
        {
            IsConnected = true,
            Provider = provider,
            Message = message,
            VerifiedCapabilities = verifiedCapabilities ?? Array.Empty<string>(),
            UnknownCapabilities = unknownCapabilities ?? Array.Empty<string>()
        };
    }

    public static ProviderConnectionResult Failed(string provider, string message)
    {
        return new ProviderConnectionResult
        {
            IsConnected = false,
            Provider = provider,
            Message = message
        };
    }
}
