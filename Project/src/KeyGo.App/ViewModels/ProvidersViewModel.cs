using System.Collections.ObjectModel;
using KeyGo.App.Infrastructure;
using KeyGo.Core.Abstractions;
using KeyGo.Core.Models;
using KeyGo.Core.Services;

namespace KeyGo.App.ViewModels;

public sealed class ProvidersViewModel : ObservableObject
{
    private readonly IProviderManager _manager;
    private readonly IReadOnlyList<IAIProvider> _providers;
    private string _selectedProviderId = string.Empty;
    private string _apiKey = string.Empty;
    private string _status = "Credentials stay on this device.";
    private bool _isBusy;

    public ProvidersViewModel(IProviderManager manager, IReadOnlyList<IAIProvider> providers, ICredentialManager credentials, MainViewModel shell)
    {
        _manager = manager; _providers = providers;
        foreach (var provider in providers) ProviderCards.Add(new ProviderCardViewModel(provider, credentials));
        SelectedProvider = ProviderCards.FirstOrDefault();
        VerifyCommand = new AsyncCommand(VerifyAsync, () => !IsBusy && SelectedProvider is not null);
    }

    public ObservableCollection<ProviderCardViewModel> ProviderCards { get; } = new();
    public ProviderCardViewModel? SelectedProvider { get => ProviderCards.FirstOrDefault(p => p.ProviderId == _selectedProviderId); private set { _selectedProviderId = value?.ProviderId ?? string.Empty; OnPropertyChanged(); OnPropertyChanged(nameof(Models)); } }
    public string ApiKey { get => _apiKey; set => SetProperty(ref _apiKey, value); }
    public string Status { get => _status; private set => SetProperty(ref _status, value); }
    public bool IsBusy { get => _isBusy; private set => SetProperty(ref _isBusy, value); }
    public IReadOnlyList<AIModel> Models => SelectedProvider?.Models ?? Array.Empty<AIModel>();
    public AsyncCommand VerifyCommand { get; }

    public void Select(ProviderCardViewModel provider) { _selectedProviderId = provider.ProviderId; OnPropertyChanged(nameof(SelectedProvider)); OnPropertyChanged(nameof(Models)); }

    private async Task VerifyAsync()
    {
        if (SelectedProvider is null) return;
        try
        {
            IsBusy = true; Status = "Saving credential and verifying provider...";
            var provider = SelectedProvider;
            if (provider is null) return;
            if (!string.IsNullOrWhiteSpace(ApiKey)) await provider.Credentials.SaveAsync(provider.ProviderId, "default", ApiKey);
            var result = await _manager.VerifyProviderAsync(provider.ProviderId);
            provider.Apply(result, await _manager.DiscoverModelsAsync(provider.ProviderId));
            Status = result.IsVerified ? $"Verified · {result.ModelsDiscovered} models discovered" : result.Message;
            OnPropertyChanged(nameof(Models));
        }
        catch (Exception ex) { Status = $"Verification failed: {ex.Message}"; }
        finally { IsBusy = false; }
    }
}

public sealed class ProviderCardViewModel : ObservableObject
{
    public ProviderCardViewModel(IAIProvider provider, ICredentialManager credentials) { ProviderId = provider.Id; DisplayName = provider.DisplayName; Credentials = credentials; }
    public string ProviderId { get; }
    public string DisplayName { get; }
    public ICredentialManager Credentials { get; }
    public string Status { get; private set; } = "Not connected";
    public bool IsVerified { get; private set; }
    public IReadOnlyList<AIModel> Models { get; private set; } = Array.Empty<AIModel>();
    public void Apply(ProviderVerificationResult result, IReadOnlyList<AIModel> models) { IsVerified = result.IsVerified; Status = result.IsVerified ? $"Connected · {models.Count} models" : result.Message; Models = models; OnPropertyChanged(nameof(Status)); OnPropertyChanged(nameof(Models)); }
}