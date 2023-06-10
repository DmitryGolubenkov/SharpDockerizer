using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SharpDockerizer.AvaloniaUI.Services;
using SharpDockerizer.AvaloniaUI.Services.Localization;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using SharpDockerizer.AppLayer.Events;

namespace SharpDockerizer.AvaloniaUI.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    #region Fields

    private readonly AppNavigator _navigator;
    private readonly LocalizationController _localizationController;
    private readonly IMessenger _messenger;

    #endregion

    #region Constructor

    public SettingsViewModel(AppNavigator navigator, LocalizationController localizationController,
        IMessenger messenger)
    {
        _navigator = navigator;
        _localizationController = localizationController;
        _messenger = messenger;

        AvaliableLocales = ApplicationLocale.GetLocales();

        CurrentLocale = AvaliableLocales
            .Where(x => x.CultureString == localizationController.CurrentLocale.CultureString).First();
    }

    #endregion

    #region Properties

    [ObservableProperty] private List<ApplicationLocale> _avaliableLocales;

    [ObservableProperty] private ApplicationLocale _currentLocale;

    partial void OnCurrentLocaleChanged(ApplicationLocale? old, ApplicationLocale value)
    {
        if (old is not null)
            if (old.CultureString != value.CultureString)
            {
                _localizationController.SetLocale(value);

            }
    }

    #endregion

    #region Methods

    [RelayCommand]
    public void GoBack()
    {
        _navigator.GoBackToPrevious();
    }

    [RelayCommand]
    public async Task RestartApp()
    {
        _messenger.Send<NeedAppRestartEvent>();
    }
    
    #endregion
}