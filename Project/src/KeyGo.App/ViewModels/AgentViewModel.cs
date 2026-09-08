using System.Collections.ObjectModel;
using KeyGo.App.Infrastructure;
using KeyGo.Core.Models;
using KeyGo.Core.Services;

namespace KeyGo.App.ViewModels;

public sealed class AgentViewModel : ObservableObject
{
    private readonly ProjectsViewModel _projects;
    private string _goal = string.Empty;
    private string _status = "Ready to investigate with read-only permissions.";
    private bool _isRunning;
    private readonly CodingAgentOrchestrator _agent;
    private CancellationTokenSource? _cancellation;
    public AgentViewModel(CodingAgentOrchestrator agent, ProjectsViewModel projects, MainViewModel shell) { _agent = agent; _projects = projects; StartCommand = new AsyncCommand(StartAsync, () => !IsRunning && !string.IsNullOrWhiteSpace(Goal) && _projects.Snapshot is not null); StopCommand = new RelayCommand(_ => _cancellation?.Cancel()); }
    public string Goal { get => _goal; set { if (SetProperty(ref _goal, value)) StartCommand.RaiseCanExecuteChanged(); } }
    public string Status { get => _status; private set => SetProperty(ref _status, value); }
    public bool IsRunning { get => _isRunning; private set { if (SetProperty(ref _isRunning, value)) StartCommand.RaiseCanExecuteChanged(); } }
    public ObservableCollection<string> Activity { get; } = new();
    public AsyncCommand StartCommand { get; }
    public RelayCommand StopCommand { get; }
    private async Task StartAsync()
    {
        if (_projects.Snapshot is null) return;
        _cancellation = new CancellationTokenSource(); IsRunning = true; Activity.Clear(); Status = "Investigating locally...";
        try
        {
            var session = await _agent.InvestigateAsync(_projects.RootPath, Goal, AgentMode.Observe, _cancellation.Token);
            foreach (var activity in session.Activity) Activity.Add($"{activity.State}: {activity.Message}");
            Status = session.State == AgentState.Completed ? "Investigation complete. No files were modified." : session.State.ToString();
        }
        catch (OperationCanceledException) { Status = "Investigation stopped."; }
        catch (Exception ex) { Status = $"Agent error: {ex.Message}"; }
        finally { IsRunning = false; _cancellation.Dispose(); _cancellation = null; }
    }
}