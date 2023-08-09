using Serilog;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SharpDockerizer.AppLayer.Utilities;

/// <summary>
/// Platform independent helper that helps with web browser actions.
/// </summary>
public static class BrowserUtility
{

    /// <summary>
    /// Opens a URL using default platform browser.
    /// </summary>
    public static void OpenBrowser(string url)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Process.Start(new ProcessStartInfo(url)
            {
                UseShellExecute = true,
            });
            Log.Information(messageTemplate: $"Opened URL: {url}");
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            Process.Start("xdg-open", url);
            Log.Information(messageTemplate: $"Opened URL: {url}");
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            Process.Start("open", url);
            Log.Information(messageTemplate: $"Opened URL: {url}");
        }
    }
}
