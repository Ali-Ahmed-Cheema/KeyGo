namespace KeyGo.Core.Models;

public sealed class CredentialRecord
{
    public required string Provider { get; init; }
    public required string Alias { get; init; }
    public string MaskedValue { get; init; } = string.Empty;
    public DateTime CreatedAtUtc { get; init; } = DateTime.UtcNow;
    public DateTime? LastUsedUtc { get; init; }
}
