namespace KeyGo.Core.Abstractions;

public interface IModel
{
    string Id { get; }
    string Provider { get; }
    string DisplayName { get; }
    IReadOnlyCollection<ModelCapability> Capabilities { get; }
    bool Supports(ModelCapability capability);
}

public enum ModelCapability
{
    TextGeneration,
    Streaming,
    Vision,
    ImageGeneration,
    AudioInput,
    AudioOutput,
    ToolCalling,
    StructuredOutput,
    Embeddings,
    Reasoning,
    LongContext,
    CodeExecution
}
