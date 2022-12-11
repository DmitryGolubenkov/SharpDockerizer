using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using SharpDockerizer.AppLayer.Contracts;
using SharpDockerizer.AppLayer.Events;
using SharpDockerizer.Core.Models;
using System.Collections.ObjectModel;
using System.Linq;

namespace SharpDockerizer.AvaloniaUI.ViewModels;
[INotifyPropertyChanged]
internal partial class SolutionViewerViewModel
{
    #region Fields

    private readonly ICurrentSolutionInfo _currentSolutionInfo;
    private readonly IMessenger _messenger;

    #endregion

    #region Constructor

    public SolutionViewerViewModel(ICurrentSolutionInfo currentSolutionInfo, IMessenger messenger)
    {
        _currentSolutionInfo = currentSolutionInfo;
        _messenger = messenger;

        _messenger.Register<SolutionLoadedEvent>(this, SolutionLoadedHandler);
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

    #endregion
}
