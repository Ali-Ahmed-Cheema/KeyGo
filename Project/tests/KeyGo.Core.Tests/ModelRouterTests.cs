using KeyGo.Core.Abstractions;
using KeyGo.Core.Models;
using KeyGo.Core.Services;
using Xunit;

namespace KeyGo.Core.Tests;

public class ModelRouterTests
{
    [Fact]
    public async Task SelectModelAsync_ReturnsHighestCapabilityModel()
    {
        var models = new IModel[]
        {
            new ProviderModel
            {
                Id = "model-a",
                Provider = "OpenAI",
                DisplayName = "Model A",
                Capabilities = new[] { ModelCapability.TextGeneration, ModelCapability.Streaming }
            },
            new ProviderModel
            {
                Id = "model-b",
                Provider = "OpenAI",
                DisplayName = "Model B",
                Capabilities = new[] { ModelCapability.TextGeneration, ModelCapability.Streaming, ModelCapability.ToolCalling }
            }
        };

        var router = new ModelRouter();
        var selected = await router.SelectModelAsync("Build code", models);

        Assert.Equal("model-b", selected);
    }
}
