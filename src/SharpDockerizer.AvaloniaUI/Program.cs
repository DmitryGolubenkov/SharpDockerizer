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
            BuildAvaloniaApp()
            .Start(AppMain, args);
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Unhandled exception occured!");
            Log.CloseAndFlush();
            throw;
        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace();
    
    // Application entry point. Avalonia is completely initialized.
    static void AppMain(Application app, string[] args)
    {
        var cts = new CancellationTokenSource();


        app.Run(cts.Token);
    }
}
        
