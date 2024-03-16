using Avalonia;
using Serilog;
using System;
using System.Threading;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;

namespace SharpDockerizer.AvaloniaUI;
internal class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        try
        {
            var builder = BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args, ShutdownMode.OnLastWindowClose);
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Unhandled exception occurred!");
            Log.CloseAndFlush();
            throw;
        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace();
}

