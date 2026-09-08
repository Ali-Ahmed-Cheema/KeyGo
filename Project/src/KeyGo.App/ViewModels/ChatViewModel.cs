using System.Collections.ObjectModel;
using KeyGo.App.Infrastructure;
using KeyGo.Core.Models;
using KeyGo.Core.Services;

namespace KeyGo.App.ViewModels;

public sealed class ChatViewModel : ObservableObject
{
    private readonly WorkspaceChatService _chat;
    private readonly ProjectsViewModel _projects;
    private readonly ProvidersViewModel _providers;
    private readonly MainViewModel _shell;
    private string _prompt = string.Empty;
    private string _status = "Open a project and connect a provider to begin.";
    private bool _isStreaming;
    private bool _isCloud = true;
    private AIModel? _selectedModel;
    private CancellationTokenSource? _cancellation;

    public ChatViewModel(WorkspaceChatService chat, ProjectsViewModel projects, ProvidersViewModel providers, MainViewModel shell)
    { _chat = chat; _projects = projects; _providers = providers; _shell = shell; SendCommand = new AsyncCommand(SendAsync, () => !IsStreaming && !string.IsNullOrWhiteSpace(Prompt) && _projects.Snapshot is not null && _providers.SelectedProvider is not null); StopCommand = new RelayCommand(_ => Stop()); }

    public ObservableCollection<ChatMessageItem> Messages { get; } = new();
    public string Prompt { get => _prompt; set { if (SetProperty(ref _prompt, value)) SendCommand.RaiseCanExecuteChanged(); } }
    public string Status { get => _status; private set => SetProperty(ref _status, value); }
    public bool IsStreaming { get => _isStreaming; private set { if (SetProperty(ref _isStreaming, value)) { SendCommand.RaiseCanExecuteChanged(); OnPropertyChanged(nameof(ShowStop)); } } }
    public bool ShowStop => IsStreaming;
    public bool IsCloud { get => _isCloud; set { if (SetProperty(ref _isCloud, value)) _shell.RefreshShell(); } }
    public IReadOnlyList<AIModel> Models => _providers.Models;
    public AIModel? SelectedModel { get => _selectedModel; set { if (SetProperty(ref _selectedModel, value)) _shell.RefreshShell(); } }
    public ProviderCardViewModel? SelectedProvider => _providers.SelectedProvider;
    public AsyncCommand SendCommand { get; }
    public RelayCommand StopCommand { get; }

    public void RefreshModels() { OnPropertyChanged(nameof(Models)); OnPropertyChanged(nameof(SelectedProvider)); SendCommand.RaiseCanExecuteChanged(); }

    private async Task SendAsync()
    {
        if (_projects.Snapshot is null || _providers.SelectedProvider is null) return;
        _cancellation = new CancellationTokenSource();
        var userPrompt = Prompt.Trim(); Prompt = string.Empty;
        Messages.Add(new ChatMessageItem("You", userPrompt, false));
        var response = new ChatMessageItem("KeyGo", string.Empty, true); Messages.Add(response);
        try
        {
            IsStreaming = true; Status = "Inspecting project context...";
            var model = SelectedModel?.Id ?? Models.FirstOrDefault()?.Id ?? "gpt-4o-mini";
            var result = await _chat.SendAsync(_projects.RootPath, _providers.SelectedProvider.ProviderId, model, userPrompt, IsCloud, text => response.Append(text), _cancellation.Token);
            Status = $"Ready · {result.Context.IncludedFiles.Count} context files · ~{result.Response.InputTokens + result.Response.OutputTokens:N0} tokens";
        }
        catch (OperationCanceledException) { Status = "Generation stopped."; }
        catch (Exception ex) { response.Append($"Unable to complete request. {ex.Message}"); Status = "Provider error"; }
        finally { IsStreaming = false; _cancellation?.Dispose(); _cancellation = null; }
    }

    private void Stop() => _cancellation?.Cancel();
}

public sealed class ChatMessageItem : ObservableObject
{
    private string _content;
    public ChatMessageItem(string author, string content, bool isAssistant) { Author = author; _content = content; IsAssistant = isAssistant; }
    public string Author { get; }
    public string Content { get => _content; private set => SetProperty(ref _content, value); }
    public bool IsAssistant { get; }
    public void Append(string text) { Content += text; }
}