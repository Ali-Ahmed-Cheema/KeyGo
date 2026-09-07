using KeyGo.Core.Abstractions;

namespace KeyGo.Core.Services;

public sealed class ConsoleAuditLogger : IAuditLogger
{
    public Task LogAsync(string message, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Console.WriteLine($"[AUDIT] {DateTime.UtcNow:O} {message}");
        return Task.CompletedTask;
    }
}
