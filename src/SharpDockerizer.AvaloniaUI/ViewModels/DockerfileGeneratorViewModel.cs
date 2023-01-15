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
using System.Threading.Tasks;
using Serilog;
using Avalonia;
using System.IO;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;

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
    [ObservableProperty]
    private bool _dockerfileWasGenerated;

    public ObservableCollection<ExposedPort> ExposedPorts { get; set; } = new ObservableCollection<ExposedPort>()
    {
        ExposedPort.Http,
    };

    public ObservableCollection<NuGetSource> NuGetSources { get; set; } = new ObservableCollection<NuGetSource>();

    #endregion

    #region Fields

    private readonly IDockerfileGenerator _dockerfileGenerator;
    private readonly IMessenger _messenger;
    private readonly ISolutionUpdater _solutionUpdater;
    private readonly ICurrentSolutionInfo _currentSolutionInfo;
    private ProjectData? _selectedProjectData;

    #endregion

    #region Constructor

    public DockerfileGeneratorViewModel(IDockerfileGenerator dockerfileGenerator,
        IMessenger messenger,
        ISolutionUpdater solutionUpdater,
        ICurrentSolutionInfo currentSolutionInfo)
    {
        _dockerfileGenerator = dockerfileGenerator;
        _messenger = messenger;
        _solutionUpdater = solutionUpdater;
        _currentSolutionInfo = currentSolutionInfo;

        _messenger.Register<ProjectSelectedEvent>(this, OnProjectSelectedHandler);
    }



    #endregion

    #region Event Handlers


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
    public async Task GenerateDockerfile()
    {
        // Refresh solution to avoid reading stale data for dockerfile.
        var solutionChanged = await _solutionUpdater.RefreshSolution();

        Log.Information($"Solution was changed: {solutionChanged}");
        // If selected project data changed - refresh data
        if (solutionChanged)
        {
            var newData = _currentSolutionInfo.Projects.FirstOrDefault(project => project.ProjectName == _selectedProjectData.ProjectName);

            if (newData is null)
            {
                // TODO: handle that selected project was removed. Notify user, solution viewer and don't crash the app.
                return;
            }

            _selectedProjectData.UpdateWithData(newData);
        }

        // Generate Dockerfile
        var result = _dockerfileGenerator.Execute(new DockerfileGeneratorInputModel
        {
            SelectedProjectData = _selectedProjectData,
            ExposedPorts = ExposedPorts.Select(x => x.Port).ToList(),
            NuGetSources = NuGetSources.ToList()
        });

        // Notify app about solution refresh
        if (solutionChanged)
        {
            _messenger.Send<SolutionRefreshedEvent>();
        }

        // Display result
        GeneratedDockerfile = result;
        DockerfileWasGenerated = true;
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

    [RelayCommand]
    public async Task CopyToClipboard()
    {
        if (GeneratedDockerfile is null)
            return;

        await Application.Current.Clipboard.SetTextAsync(GeneratedDockerfile);

        // TODO: Notification
    }

    [RelayCommand]
    public async Task SaveToProjectFolder()
    {
        if (GeneratedDockerfile is null)
            return;

        await File.WriteAllTextAsync(
            Path.Combine(Path.GetDirectoryName(_selectedProjectData.AbsolutePathToProjFile), "Dockerfile"),
            GeneratedDockerfile);

        // TODO: Dialog "Overwrite existing file?"
        // TODO: Notification
    }

    [RelayCommand]
    public async Task SaveToChosenFolder()
    {
        // TODO: Fix some bug with Downloads folder
        if (Application.Current.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktopLifetime)
        {
            return;
        }

        var result = await desktopLifetime.MainWindow.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions()
        {
            Title = "Choose folder, where Dockerfile will be saved",
            AllowMultiple = false,
        });

        if (result[0].TryGetUri(out var pathUri))
        {
            await File.WriteAllTextAsync(
            Path.Combine(pathUri.AbsolutePath, "Dockerfile"),
            GeneratedDockerfile);
        }

        // TODO: Dialog "Overwrite existing file?"
        // TODO: Notification
    }

    #endregion
}
