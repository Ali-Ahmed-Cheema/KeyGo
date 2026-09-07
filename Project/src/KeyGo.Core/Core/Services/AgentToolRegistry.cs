using KeyGo.Core.Models;

namespace KeyGo.Core.Services;

public sealed class AgentToolRegistry
{
    private readonly Dictionary<string, ToolDescriptor> _tools = new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyCollection<ToolDescriptor> Tools => _tools.Values.ToArray();

    public void Register(ToolDescriptor descriptor)
    {
        if (string.IsNullOrWhiteSpace(descriptor.Id)) throw new ArgumentException("Tool ID is required.", nameof(descriptor));
        _tools[descriptor.Id] = descriptor;
    }

    public bool TryGet(string toolId, out ToolDescriptor? descriptor) => _tools.TryGetValue(toolId, out descriptor);

    public static AgentToolRegistry CreateDefault()
    {
        var registry = new AgentToolRegistry();
        foreach (var tool in new[]
        {
            new ToolDescriptor { Id = "ReadFile", Description = "Read a project file.", RequiredPermission = AgentPermission.Read, RiskLevel = ToolRiskLevel.Low },
            new ToolDescriptor { Id = "SearchFiles", Description = "Search project text.", RequiredPermission = AgentPermission.Read, RiskLevel = ToolRiskLevel.Low },
            new ToolDescriptor { Id = "GetProjectTree", Description = "Inspect indexed project files.", RequiredPermission = AgentPermission.Read, RiskLevel = ToolRiskLevel.Low },
            new ToolDescriptor { Id = "ModifyFile", Description = "Apply an approved file change.", RequiredPermission = AgentPermission.Write, RiskLevel = ToolRiskLevel.High },
            new ToolDescriptor { Id = "DeleteFile", Description = "Delete an approved project file.", RequiredPermission = AgentPermission.Delete, RiskLevel = ToolRiskLevel.Critical },
            new ToolDescriptor { Id = "RunTests", Description = "Run an approved test command.", RequiredPermission = AgentPermission.Execute, RiskLevel = ToolRiskLevel.Medium }
        }) registry.Register(tool);
        return registry;
    }
}
