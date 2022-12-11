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

namespace SharpDockerizer.AvaloniaUI.ViewModels;
[INotifyPropertyChanged]
internal partial class TopBarViewModel
{
    private ISolutionLoader _solutionLoader;
    private readonly IMessenger _messenger;

    public TopBarViewModel(ISolutionLoader solutionLoader, IMessenger messenger)
    {
        _solutionLoader = solutionLoader;
        _messenger = messenger;
    }


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
                Title = "Choose .sln file",
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
                result[0].TryGetUri(out var uri);
                await _solutionLoader.LoadSolution(uri.AbsolutePath);
                _messenger.Send<SolutionLoadedEvent>();
            }
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
}
