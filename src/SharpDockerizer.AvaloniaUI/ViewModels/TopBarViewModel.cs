using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Threading.Tasks;
using SharpDockerizer.AppLayer.Contracts;
using CommunityToolkit.Mvvm.Messaging;
using SharpDockerizer.AppLayer.Events;
using SharpDockerizer.AppLayer.Utilities;
using SharpDockerizer.AppLayer.Models;
using System.IO;
using SharpDockerizer.AvaloniaUI.Properties;
using SharpDockerizer.AvaloniaUI.Services;

namespace SharpDockerizer.AvaloniaUI.ViewModels;

internal partial class TopBarViewModel : ObservableObject
{
    #region Fields

    private readonly ISolutionLoader _solutionLoader;
    private readonly IMessenger _messenger;
    private readonly IRecentlyOpenedSolutionsService _recentlyOpenedSolutionsService;
    private readonly ICurrentSolutionInfo _currentSolutionInfo;
    private readonly AppNavigator _navigator;

    #endregion

    #region Constructor

    public TopBarViewModel(ISolutionLoader solutionLoader, IMessenger messenger, IRecentlyOpenedSolutionsService recentlyOpenedSolutionsService, ICurrentSolutionInfo currentSolutionInfo, AppNavigator navigator)
    {
        _solutionLoader = solutionLoader;
        _messenger = messenger;
        _recentlyOpenedSolutionsService = recentlyOpenedSolutionsService;
        _currentSolutionInfo = currentSolutionInfo;
        _navigator = navigator;
        // Load recent solutions. Also check if there are no recent solutions.

        var recentSolutions = recentlyOpenedSolutionsService.GetSolutions();
        if (recentSolutions is not null && recentSolutions.Count > 0)
        {
            RecentSolutions = recentSolutions;
            RecentSolutionsEnabled = true;
        }
    }

    #endregion

    #region Observable Properties

    [ObservableProperty]
    private List<RecentlyOpenedSolution> _recentSolutions;
    [ObservableProperty]
    private bool _recentSolutionsEnabled;
    #endregion

    #region Relay Commands

    /// <summary>
    /// Loads solution using .sln file path
    /// </summary>
    [RelayCommand]
    internal async Task LoadSolution()
    {
        if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
        {
            var result = await desktopLifetime.MainWindow.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions()
            {
                Title = Resources.ChooseSlnFile,
                AllowMultiple = false,
                FileTypeFilter = new List<FilePickerFileType>()
                    {
                        new FilePickerFileType(".sln File")
                        {
                           Patterns = new [] {"*.sln" },
                           MimeTypes= new[] { "application/xml", "text/xml" }
                        },
                    }
            });

            if (result != null)
            {
                var uri = result[0].Path;
                await _solutionLoader.LoadSolution(uri.AbsolutePath);
                await _recentlyOpenedSolutionsService.Add(new RecentlyOpenedSolution()
                {
                    Name =_currentSolutionInfo.CurrentSolution.Name,
                    AbsolutePath = _currentSolutionInfo.CurrentSolution.SolutionFilePath
                });

                RecentSolutions =  _recentlyOpenedSolutionsService.GetSolutions();
                RecentSolutionsEnabled = true;

                _messenger.Send<SolutionLoadedEvent>();
            }
        }
    }

    [RelayCommand]
    internal async Task OpenRecentSolution(object path)
    {
        try
        {
            await _solutionLoader.LoadSolution(path as string);
            _messenger.Send<SolutionLoadedEvent>();
        }
        catch (IOException ex)
        {
            // TODO: Notification
            _recentlyOpenedSolutionsService.RemoveWithPath(path as string);
        }
    }

    [RelayCommand]
    internal void CloseApp()
    {
        if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
        {
            desktopLifetime.Shutdown();
        }
    }

    [RelayCommand]
    internal void OpenGitHubPage()
    {
        BrowserUtility.OpenBrowser("https://github.com/DmitryGolubenkov/SharpDockerizer");
    }

    [RelayCommand]
    internal void OpenSettings()
    {
        _navigator.NavigateTo<SettingsViewModel>();
    }

    #endregion
    
}