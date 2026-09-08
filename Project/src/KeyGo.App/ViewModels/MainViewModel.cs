using System.Collections.ObjectModel;
using KeyGo.App.Infrastructure;
using KeyGo.App.Services;
using KeyGo.Core.Models;

namespace KeyGo.App.ViewModels;

public sealed class MainViewModel : ObservableObject
{
    private object _currentPage;
    private string _activeSection = "Home";

    public MainViewModel(AppHost host)
    {
        Projects = new ProjectsViewModel(host.Workspace, new WindowsFolderPickerService(), this);
        Home = new HomeViewModel(this);
        Providers = new ProvidersViewModel(host.ProviderManager, host.Providers, host.Credentials, this);
        Chat = new ChatViewModel(host.Chat, Projects, Providers, this);
        Agent = new AgentViewModel(host.Agent, Projects, this);
        _currentPage = Home;
        NavigateCommand = new RelayCommand(parameter => Navigate(parameter as string ?? "Home"));
        OpenProjectCommand = new AsyncCommand(Projects.OpenProjectAsync);
    }

    public HomeViewModel Home { get; }
    public ProjectsViewModel Projects { get; }
    public ProvidersViewModel Providers { get; }
    public ChatViewModel Chat { get; }
    public AgentViewModel Agent { get; }
    public object CurrentPage { get => _currentPage; private set => SetProperty(ref _currentPage, value); }
    public string ActiveSection { get => _activeSection; private set => SetProperty(ref _activeSection, value); }
    public string ProjectName => Projects.ProjectName;
    public string ModelLabel => Chat.SelectedModel?.DisplayName ?? "Auto";
    public string PrivacyLabel => Chat.IsCloud ? "CLOUD" : "LOCAL";
    public RelayCommand NavigateCommand { get; }
    public AsyncCommand OpenProjectCommand { get; }

    public void Navigate(string section)
    {
        ActiveSection = section;
        CurrentPage = section switch
        {
            "Projects" => Projects,
            "Chat" => Chat,
            "Providers" => Providers,
            "Models" => Providers,
            "Agent" => Agent,
            "Usage" => new InfoPageViewModel("Usage & Costs", "Usage data will appear here after a provider request is completed.", "SESSION USAGE"),
            "Security" => new InfoPageViewModel("Security", "Credentials are kept in the local credential manager abstraction. Secret exclusion is reported by the context inspector.", "LOCAL-FIRST SECURITY"),
            "Settings" => new InfoPageViewModel("Settings", "Appearance, privacy, permissions, and provider settings will be managed here.", "SETTINGS"),
            _ => Home
        };
        OnPropertyChanged(nameof(ProjectName));
    }

    public void RefreshShell()
    {
        OnPropertyChanged(nameof(ProjectName));
        OnPropertyChanged(nameof(ModelLabel));
        OnPropertyChanged(nameof(PrivacyLabel));
    }
}

public sealed class RelayCommand : System.Windows.Input.ICommand
{
    private readonly Action<object?> _execute;
    public RelayCommand(Action<object?> execute) => _execute = execute;
    public event EventHandler? CanExecuteChanged;
    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    public bool CanExecute(object? parameter) => true;
    public void Execute(object? parameter) => _execute(parameter);
}

public sealed class HomeViewModel : ObservableObject
{
    public HomeViewModel(MainViewModel shell) { OpenProjectCommand = new AsyncCommand(shell.Projects.OpenProjectAsync); NewChatCommand = new RelayCommand(_ => shell.Navigate("Chat")); }
    public AsyncCommand OpenProjectCommand { get; }
    public RelayCommand NewChatCommand { get; }
}

public sealed class InfoPageViewModel
{
    public InfoPageViewModel(string title, string description, string eyebrow) { Title = title; Description = description; Eyebrow = eyebrow; }
    public string Title { get; }
    public string Description { get; }
    public string Eyebrow { get; }
}