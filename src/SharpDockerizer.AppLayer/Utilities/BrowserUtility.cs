using Serilog;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SharpDockerizer.AppLayer.Utilities;
public static class BrowserUtility
{
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
