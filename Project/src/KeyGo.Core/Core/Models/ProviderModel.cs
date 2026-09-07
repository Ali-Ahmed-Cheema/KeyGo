using KeyGo.Core.Abstractions;

namespace KeyGo.Core.Models;

public sealed class ProviderModel : IModel
{
    public required string Id { get; init; }
    public required string Provider { get; init; }
    public required string DisplayName { get; init; }
    public required IReadOnlyCollection<ModelCapability> Capabilities { get; init; }

    public bool Supports(ModelCapability capability)
    {
        return Capabilities.Contains(capability);
    }
}
