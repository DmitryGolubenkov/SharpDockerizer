using System.Collections.Generic;

namespace SharpDockerizer.AvaloniaUI.Services.Localization;
public class ApplicationLocale
{
    /// <summary>
    /// Culture string of the locale
    /// </summary>
    public string CultureString { private set; get; }
    /// <summary>
    /// String displayed to user for this locale
    /// </summary>
    public string DisplayedString { private set; get; }

    /// <summary>
    /// Returns avaliable locales that can be loaded by application
    /// </summary>
    /// <returns></returns>
    public static List<ApplicationLocale> GetLocales()
    {
        return new List<ApplicationLocale>()
        {
            new ApplicationLocale()
            {
                CultureString = "ru-RU",
                DisplayedString="Русский"
            },
            new ApplicationLocale()
            {
                CultureString = "en-US",
                DisplayedString="English"
            }
        };
    }
}
