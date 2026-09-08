using System.Collections.ObjectModel;
using KeyGo.App.Infrastructure;
using KeyGo.App.Services;
using KeyGo.Core.Models;
using KeyGo.Core.Services;

namespace KeyGo.App.ViewModels;

public sealed class ProjectsViewModel : ObservableObject
{
    private readonly ProjectWorkspaceService _workspace;
    private readonly IFolderPickerService _picker;
    private readonly MainViewModel _shell;
    private string _projectName = "No project open";
    private string _rootPath = string.Empty;
    private string _status = "Choose a local project to begin.";
    private bool _isLoading;
    private ProjectIndexSnapshot? _snapshot;

    public ProjectsViewModel(ProjectWorkspaceService workspace, IFolderPickerService picker, MainViewModel shell)
    { _workspace = workspace; _picker = picker; _shell = shell; OpenProjectCommand = new AsyncCommand(OpenProjectAsync, () => !IsLoading); RefreshCommand = new AsyncCommand(RefreshAsync, () => !IsLoading && !string.IsNullOrEmpty(RootPath)); }

    public ObservableCollection<ProjectFileItem> Files { get; } = new();
    public string ProjectName { get => _projectName; private set { if (SetProperty(ref _projectName, value)) _shell.RefreshShell(); } }
    public string RootPath { get => _rootPath; private set => SetProperty(ref _rootPath, value); }
    public string Status { get => _status; private set => SetProperty(ref _status, value); }
    public bool IsLoading { get => _isLoading; private set { if (SetProperty(ref _isLoading, value)) { OpenProjectCommand.RaiseCanExecuteChanged(); RefreshCommand.RaiseCanExecuteChanged(); } } }
    public ProjectIndexSnapshot? Snapshot => _snapshot;
    public AsyncCommand OpenProjectCommand { get; }
    public AsyncCommand RefreshCommand { get; }

    public async Task OpenProjectAsync()
    {
        var path = _picker.PickFolder();
        if (path is not null) await LoadAsync(path);
    }

    public async Task RefreshAsync() { if (!string.IsNullOrEmpty(RootPath)) await LoadAsync(RootPath); }

    private async Task LoadAsync(string path)
    {
        try
        {
            IsLoading = true; Status = "Indexing project...";
            _snapshot = await _workspace.IndexAsync(path);
            RootPath = _snapshot.Project.RootPath; ProjectName = _snapshot.Project.Name;
            Files.Clear();
            foreach (var file in _snapshot.Files) Files.Add(new ProjectFileItem(file));
            Status = $"{Files.Count:N0} files indexed · {string.Join(" · ", _snapshot.Project.Technologies)}";
            _shell.RefreshShell();
        }
        catch (Exception ex) { Status = $"Unable to open project: {ex.Message}"; }
        finally { IsLoading = false; }
    }
}

public sealed class ProjectFileItem
{
    public ProjectFileItem(ProjectFileEntry file) { RelativePath = file.RelativePath; Kind = file.Kind.ToString(); LineCount = file.LineCount?.ToString() ?? ""; }
    public string RelativePath { get; }
    public string Kind { get; }
    public string LineCount { get; }
}