using CommunityToolkit.Mvvm.ComponentModel;
using SharpDockerizer.AvaloniaUI.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpDockerizer.AvaloniaUI.ViewModels;

internal partial class MainWindowViewModel : ObservableObject
{

    [ObservableProperty]
    private ObservableObject _currentPageViewModel;

    private List<ObservableObject> _pages;
    public MainWindowViewModel(AppNavigator appNavigator, MainApplicationViewViewModel mainApplicationViewViewModel, SettingsViewModel settingsViewModel)
    {
        appNavigator.onNavigateTo += OnNavigateToHandler;
        _pages = new List<ObservableObject>()
        {
            mainApplicationViewViewModel,
            settingsViewModel,
        };

        CurrentPageViewModel = mainApplicationViewViewModel;
    }

    private void OnNavigateToHandler(Type type)
    {
        var viewModel = _pages.Where(x => x.GetType() == type).FirstOrDefault();
        if (viewModel is not null)
        {
            CurrentPageViewModel = viewModel;
        }
    }
}
