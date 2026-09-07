namespace KeyGo.Core.Abstractions;

public interface IPermissionService
{
    Task<bool> CanExecuteAsync(string actionName, string? resourcePath = null, CancellationToken cancellationToken = default);
}
