using SharpDockerizer.AvaloniaUI.Services.Localization;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace SharpDockerizer.AvaloniaUI.Services;
public class OptionsService
{
    private const string optionsFileName = "settings.json";

    private string optionsFilePath { get => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, optionsFileName); }
    public ApplicationOptions ApplicationOptions { get; private set; }

    public OptionsService()
    {
        LoadOptionsOnStartUp();
    }

    private void LoadOptionsOnStartUp()
    {
        try
        {
            if (File.Exists(optionsFilePath))
                ApplicationOptions = JsonSerializer.Deserialize<ApplicationOptions>(optionsFilePath);
            else
                CreateDefaultOptions();
        }
        catch
        {
            CreateDefaultOptions();
        }
    }

    private void CreateDefaultOptions()
    {
        var avaliableLocales = ApplicationLocale.GetLocales();
        ApplicationOptions = new ApplicationOptions()
        {
            Locale = avaliableLocales.FirstOrDefault(locale => locale.CultureString == CultureInfo.CurrentUICulture.Name)
        };

        SaveOptions();
    }

    private void SaveOptions()
    {
        var settings = JsonSerializer.Serialize(ApplicationOptions);
        File.WriteAllText(optionsFilePath, settings);
    }
}


public class ApplicationOptions
{
    public ApplicationLocale Locale { get; set; }

}