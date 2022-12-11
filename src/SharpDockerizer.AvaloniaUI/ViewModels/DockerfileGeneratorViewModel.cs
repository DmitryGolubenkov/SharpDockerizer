using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SharpDockerizer.AppLayer.Contracts;
using SharpDockerizer.AppLayer.Events;
using SharpDockerizer.AppLayer.Models;
using SharpDockerizer.Core.Models;
using SharpDockerizer.AvaloniaUI.Models;
using System.Collections.ObjectModel;
using System.Linq;

namespace SharpDockerizer.AvaloniaUI.ViewModels;
[INotifyPropertyChanged]
internal partial class DockerfileGeneratorViewModel
{
    #region Properties

    [ObservableProperty]
    private bool _isProjectSelected;
    [ObservableProperty]
    private string? _selectedProjectName;
    [ObservableProperty]
    private string? _generatedDockerfile;

    public ObservableCollection<ExposedPort> ExposedPorts { get; set; } = new ObservableCollection<ExposedPort>()
    {
        ExposedPort.Http,
    };

    public ObservableCollection<NuGetSource> NuGetSources { get; set; } = new ObservableCollection<NuGetSource>();

    #endregion

    #region Fields

    private readonly IDockerfileGenerator _dockerfileGenerator;
    private readonly IMessenger _messenger;
    private ProjectData? _selectedProjectData;

    #endregion

    #region Constructor

    public DockerfileGeneratorViewModel(IDockerfileGenerator dockerfileGenerator, IMessenger messenger)
    {
        _dockerfileGenerator = dockerfileGenerator;
        _messenger = messenger;
        _messenger.Register<ProjectSelectedEvent>(this, OnProjectSelectedHandler);
        _messenger.Register<SolutionLoadedEvent>(this, OnSolutionLoadedEvent);
    }

    #endregion

    #region Event Handlers

    private void OnSolutionLoadedEvent(object recipient, SolutionLoadedEvent message)
    {
        /*IsProjectSelected = false;
        _selectedProjectData = null;
        SelectedProjectName = null;*/
    }

    private void OnProjectSelectedHandler(object sender, ProjectSelectedEvent? args)
    {
        if (args.SelectedProject is not null)
        {
            IsProjectSelected = true;
            _selectedProjectData = args.SelectedProject;
            SelectedProjectName = args.SelectedProject.ProjectName;
        }
        else
        {
            IsProjectSelected = false;
            _selectedProjectData = null;
            SelectedProjectName = null;
        }
    }
    #endregion

    #region Relay Commands

    [RelayCommand]
    public void GenerateDockerfile()
    {
        if (_selectedProjectData is null)
            return;


        var result = _dockerfileGenerator.Execute(new DockerfileGeneratorInputModel
        {
            SelectedProjectData = _selectedProjectData,
            ExposedPorts = ExposedPorts.Select(x => x.Port).ToList(),
            NuGetSources = NuGetSources.ToList()
        });

        GeneratedDockerfile = result;
    }

    [RelayCommand]
    public void AddExposedPort()
    {
        ExposedPorts.Add(new ExposedPort() { Port = 0 });
        OnPropertyChanged(nameof(ExposedPorts));
    }

    [RelayCommand]
    public void RemoveExposedPort(object port)
    {
        ExposedPorts.Remove(port as ExposedPort);
        OnPropertyChanged(nameof(ExposedPorts));
    }

    [RelayCommand]
    public void AddNuGetSource()
    {
        NuGetSources.Add(new NuGetSource()
        {
            Name = "NugetSource",
            Link = "https://linktonugetsource.com/index.json",
            AuthenticationRequired = false
        });
        OnPropertyChanged(nameof(NuGetSources));
    }

    [RelayCommand]
    public void RemoveNuGetSource(object source)
    {
        NuGetSources.Remove(source as NuGetSource);
        OnPropertyChanged(nameof(NuGetSources));
    }

    #endregion
}
