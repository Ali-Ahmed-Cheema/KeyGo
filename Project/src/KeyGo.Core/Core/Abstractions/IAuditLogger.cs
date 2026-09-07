namespace KeyGo.Core.Abstractions;

public interface IAuditLogger
{
    Task LogAsync(string message, CancellationToken cancellationToken = default);
}
