using KeyGo.Core.Services;
using Xunit;

namespace KeyGo.Core.Tests;

public class CredentialVaultServiceTests
{
    [Fact]
    public async Task SaveCredentialAsync_StoresAndMasksCredential()
    {
        var manager = new WindowsCredentialManager();
        var vault = new CredentialVaultService(manager);

        await vault.SaveCredentialAsync("openai", "default", "sk-test-secret");

        var secret = await vault.GetCredentialAsync("openai", "default");
        var records = await vault.ListCredentialsAsync("openai");

        Assert.Equal("sk-test-secret", secret);
        Assert.NotEmpty(records);
        Assert.Contains(records, r => r.Alias == "default");
        Assert.EndsWith("cret", vault.MaskSecret("sk-test-secret"));
    }
}
