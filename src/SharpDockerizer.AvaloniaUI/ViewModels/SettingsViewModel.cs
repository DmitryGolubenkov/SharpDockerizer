using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SharpDockerizer.AvaloniaUI.Services;
using SharpDockerizer.AvaloniaUI.Services.Localization;
using System.Collections.Generic;
using System.Linq;

namespace SharpDockerizer.AvaloniaUI.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    #region Fields

    private readonly AppNavigator _navigator;
    private readonly LocalizationController _localizationController;

    #endregion

    #region Constructor

    public SettingsViewModel(AppNavigator navigator, LocalizationController localizationController)
    {
        _navigator = navigator;
        _localizationController = localizationController;

        AvaliableLocales = ApplicationLocale.GetLocales();

        CurrentLocale = AvaliableLocales.Where(x => x.CultureString == localizationController.CurrentLocale.CultureString).First();
    }

    #endregion

    #region Properties

    [ObservableProperty]
    private List<ApplicationLocale> _avaliableLocales;

    [ObservableProperty]
    private ApplicationLocale _currentLocale;
    partial void OnCurrentLocaleChanged(ApplicationLocale value)
    {
        if (_localizationController.CurrentLocale != value)
            _localizationController.SetLocale(value);

    }

    #endregion

    #region Methods

    [RelayCommand]
    public void GoBack()
    {
        _navigator.GoBackToPrevious();
    }

    #endregion
}
