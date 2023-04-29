using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SharpDockerizer.AppLayer.Contracts;
using SharpDockerizer.AppLayer.Events;
using SharpDockerizer.Core.Models;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace SharpDockerizer.AvaloniaUI.ViewModels;

public partial class SolutionViewerViewModel : ObservableObject
{
    #region Fields

    private readonly ICurrentSolutionInfo _currentSolutionInfo;
    private readonly IMessenger _messenger;
    private readonly ISolutionUpdater _solutionUpdater;

    #endregion

    #region Constructor

    public SolutionViewerViewModel(ICurrentSolutionInfo currentSolutionInfo, IMessenger messenger, ISolutionUpdater solutionUpdater)
    {
        _currentSolutionInfo = currentSolutionInfo;
        _messenger = messenger;
        _solutionUpdater = solutionUpdater;
        _messenger.Register<SolutionLoadedEvent>(this, SolutionLoadedHandler);
        _messenger.Register<SolutionRefreshedEvent>(this, SolutionRefreshedHandler);
    }
    #endregion

    #region Properties

    /// <summary>
    /// Was solution loaded?
    /// </summary>
    [ObservableProperty]
    private bool _isSolutionLoaded;

    /// <summary>
    /// Project that was selected in UI
    /// </summary>
    [ObservableProperty]
    private ProjectData _selectedProject;

    partial void OnSelectedProjectChanged(ProjectData value)
    {
        _messenger.Send(new ProjectSelectedEvent()
        {
            SelectedProject = value
        });
    }

    /// <summary>
    /// Projects in .NET solution
    /// </summary>
    public ObservableCollection<ProjectData> SolutionProjects { get; set; } = new ObservableCollection<ProjectData>();

    #endregion

    #region Event Handlers

    /// <summary>
    /// This handler is executed when a solution was loaded into application.
    /// </summary>
    private void SolutionLoadedHandler(object recipient, SolutionLoadedEvent message)
    {
        SolutionProjects.Clear();
        if (_currentSolutionInfo.Projects.Any())
        {
            foreach (ProjectData project in _currentSolutionInfo.Projects)
            {
                SolutionProjects.Add(project);
            }
            IsSolutionLoaded = true;
        }
        else
        {
            IsSolutionLoaded = false;
        }
    }

    /// <summary>
    /// Handles solution refresh event.
    /// </summary>
    private void SolutionRefreshedHandler(object recipient, SolutionRefreshedEvent message)
    {
        foreach (var newProjectData in _currentSolutionInfo.Projects)
        {
            // Check if such project already exists
            var existingData = SolutionProjects.Where(x => x.ProjectName == newProjectData.ProjectName).FirstOrDefault();
            if (existingData is not null)
            {
                // Update existing reference.
                // By doing that, we don't break selected project reference.
                existingData.UpdateWithData(newProjectData);
            }
            else
            {
                // This is a new project, so add it to solution data.
                SolutionProjects.Add(newProjectData);
            }
        }

        // Remove deleted projects
        for (int i = 0; i < SolutionProjects.Count; i++)
        {
            ProjectData? existingData = SolutionProjects[i];
            if (!_currentSolutionInfo.Projects.Where(newProj => newProj.ProjectName == existingData.ProjectName).Any())
            {
                SolutionProjects.RemoveAt(i);
                i--;
            }
        }
    }

    #endregion

    #region Commands

    [RelayCommand]
    public async Task RefreshSolution()
    {
        if (IsSolutionLoaded && await _solutionUpdater.RefreshSolution())
        {
            _messenger.Send<SolutionRefreshedEvent>();
        }
    }

    #endregion
}
