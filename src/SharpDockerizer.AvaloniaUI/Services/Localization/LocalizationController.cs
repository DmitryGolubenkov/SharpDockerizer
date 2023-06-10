using System.Globalization;
using System.Threading;
using CommunityToolkit.Mvvm.Messaging;

namespace SharpDockerizer.AvaloniaUI.Services.Localization;
public class LocalizationController
{
    private readonly OptionsService _optionsService;
    private readonly IMessenger _messenger;

    public LocalizationController(OptionsService optionsService)
    {
        _optionsService = optionsService;
        CurrentLocale = optionsService.ApplicationOptions.Locale;
    }

    /// <summary>
    /// Current locale that is used by application
    /// </summary>
    public ApplicationLocale CurrentLocale { get; private set; }

    /// <summary>
    /// Sets current application locale.
    /// </summary>
    /// <param name="locale">Locale to use</param>
    public void SetLocale(ApplicationLocale locale)
    {
        CultureInfo ci = new CultureInfo(locale.CultureString);
        if (ci.Name == Thread.CurrentThread.CurrentCulture.Name)
        {
            return;
        }
        
        Thread.CurrentThread.CurrentCulture = ci;
        Thread.CurrentThread.CurrentUICulture = ci;
        _optionsService.ApplicationOptions.Locale = locale;
        CurrentLocale = locale;
    }
}

