using KeyGo.Core.Abstractions;
using KeyGo.Core.Models;

namespace KeyGo.Core.Services;

public sealed class AgentPermissionService : IPermissionService
{
    private readonly AgentMode _mode;
    private readonly HashSet<AgentPermission> _sessionGrants = new();

    public AgentPermissionService(AgentMode mode = AgentMode.Observe)
    {
        _mode = mode;
    }

    public AgentMode Mode => _mode;

    public void GrantForSession(AgentPermission permission)
    {
        _sessionGrants.Add(permission);
    }

    public Task<bool> CanExecuteAsync(string actionName, string? resourcePath = null, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var permission = ParsePermission(actionName);
        var allowed = permission == AgentPermission.Read || (_mode != AgentMode.Observe && _sessionGrants.Contains(permission));
        return Task.FromResult(allowed);
    }

    private static AgentPermission ParsePermission(string actionName)
    {
        if (Enum.TryParse<AgentPermission>(actionName, ignoreCase: true, out var permission)) return permission;
        return actionName.ToLowerInvariant() switch
        {
            "readfile" or "searchfiles" or "searchsymbols" or "getprojecttree" or "getprojectmetadata" or "getgitstatus" or "getgitdiff" or "getgithistory" or "findreferences" or "readdirectory" => AgentPermission.Read,
            "createfile" or "modifyfile" or "movefile" or "renamefile" => AgentPermission.Write,
            "deletefile" => AgentPermission.Delete,
            "runcommand" or "runtests" or "runbuild" => AgentPermission.Execute,
            "gitcommit" or "gitreset" or "gitclean" => AgentPermission.GitWrite,
            "gitpush" => AgentPermission.NetworkWrite,
            _ => AgentPermission.NetworkWrite
        };
    }
}
