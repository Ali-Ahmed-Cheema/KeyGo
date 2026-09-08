using KeyGo.Core.Abstractions;
using KeyGo.Core.Models;

namespace KeyGo.Core.Services;

public sealed class ConsoleAuditLogger : IAuditLogger
{
    public Task LogAsync(string message, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var sanitized = SecretSanitizer.Sanitize(message);
        Console.WriteLine($"[AUDIT] {DateTime.UtcNow:O} {sanitized}");
        return Task.CompletedTask;
    }
}
