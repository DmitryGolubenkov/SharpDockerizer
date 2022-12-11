using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.Messaging;
using SharpDockerizer.AppLayer.Contracts;
using SharpDockerizer.AppLayer.Generation;
using SharpDockerizer.AppLayer.Services.Project;
using SharpDockerizer.AppLayer.Services.Solution;
using SharpDockerizer.AvaloniaUI;
using SharpDockerizer.AvaloniaUI.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;

namespace SharpDockerizer.AvaloniaUI;
public partial class App : Application
{
    private IServiceProvider _services;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        IServiceCollection serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection);

        _services = serviceCollection.BuildServiceProvider();
        Log.Information("ServiceProvider initialized");

        // Used in DI and MVVM
        DISource.Resolver = _services.GetService;

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();
        }
        Log.Information("Application started");
        base.OnFrameworkInitializationCompleted();
    }

    private void ConfigureServices(IServiceCollection serviceCollection)
    {
        // ViewModels
        /*foreach (var viewModelType in GetType().Assembly.GetTypes().Where(type => type.Namespace == "SharpDockerizer.AvaloniaUI.ViewModels"))
        {
            //serviceCollection.AddTransient<viewModelType>();
        }*/
        ConfigureLogging(serviceCollection);

        serviceCollection.AddTransient<DockerfileGeneratorViewModel>();
        serviceCollection.AddTransient<MainWindowViewModel>();
        serviceCollection.AddTransient<SolutionViewerViewModel>();
        serviceCollection.AddTransient<TopBarViewModel>();

        serviceCollection.AddSingleton<IMessenger, StrongReferenceMessenger>();
        serviceCollection.AddSingleton<ICurrentSolutionInfo, CurrentSolutionInfo>();
        serviceCollection.AddTransient<ISolutionLoader, SolutionLoader>();
        serviceCollection.AddTransient<IProjectDataExporter, ProjectDataExporter>();
        serviceCollection.AddTransient<IDockerfileGenerator, DockerfileGenerator>();
        serviceCollection.AddTransient<IAspNetDockerImageVersionSelector, AspNetDockerImageVersionSelector>();
        serviceCollection.AddTransient<IDotNetSdkImageVersionSelector, DotNetSdkImageVersionSelector>();
        serviceCollection.AddTransient<IProjectDependenciesExporter, ProjectDependenciesExporter>();
    }

    private void ConfigureLogging(IServiceCollection services)
    {
        var loggerConfiguration = new LoggerConfiguration()
           .WriteTo.File("logs/app.log", rollingInterval: RollingInterval.Day, fileSizeLimitBytes: 3145728);

        ILogger log = loggerConfiguration.CreateLogger();
        Log.Logger = log;
        services.AddSingleton(log);
    }

}
