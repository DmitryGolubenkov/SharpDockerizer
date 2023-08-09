using Avalonia;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.Messaging;
using SharpDockerizer.AppLayer.Contracts;
using SharpDockerizer.AppLayer.Generation;
using SharpDockerizer.AppLayer.Services.Project;
using SharpDockerizer.AppLayer.Services.Solution;
using Serilog;
using Autofac;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using SharpDockerizer.AppLayer.Events;
using SharpDockerizer.AvaloniaUI.Services;
using SharpDockerizer.AvaloniaUI.Services.Localization;
using SharpDockerizer.AppLayer.Services.Templates;

namespace SharpDockerizer.AvaloniaUI;

public partial class App : Application
{
    #region Fields

    private IContainer? _serviceContainer;
    private MainWindow? _mainWindow;

    #endregion

    #region Avalonia Methods

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // Configure required services
        var builder = new ContainerBuilder();
        ConfigureServices(builder);
        var services = builder.Build();
        _serviceContainer = services;

        // Resolver is used in DI and MVVM to resolve viewmodels for views.
        DISource.Resolver = services.Resolve;

        // Resolve and show main window
        var mainWindow = services.Resolve<MainWindow>();
        _mainWindow = mainWindow;
        mainWindow.Show();

        var desktopLifetime = new ClassicDesktopStyleApplicationLifetime();
        desktopLifetime.MainWindow = mainWindow;
        desktopLifetime.ShutdownMode = ShutdownMode.OnLastWindowClose;
        ApplicationLifetime = desktopLifetime;

        Log.Information("Application started");

        var messenger = services.Resolve<IMessenger>();
        messenger.Register<NeedAppRestartEvent>(this, RestartApp);

        base.OnFrameworkInitializationCompleted();
    }

    #endregion


    private void ConfigureServices(ContainerBuilder builder)
    {
        // Logging
        ConfigureLogging(builder);

        //Register all view models
        builder.RegisterAssemblyTypes(typeof(App).Assembly)
            .Where(t => t.Name.EndsWith("ViewModel"))
            .AsSelf()
            .AsImplementedInterfaces();

        // UI Services
        builder.RegisterType<StrongReferenceMessenger>().As<IMessenger>().SingleInstance();
        builder.RegisterType<AppNavigator>().AsSelf().SingleInstance();
        builder.RegisterType<ClipboardService>().As<IClipboardService>();

        builder.RegisterType<OptionsService>().AsSelf().SingleInstance();
        builder.RegisterType<LocalizationController>().AsSelf().SingleInstance();
        builder.RegisterType<MainWindow>().AsSelf();

        // Register other services required for application to function
        builder.RegisterType<CurrentSolutionInfo>().As<ICurrentSolutionInfo>().SingleInstance();
        builder.RegisterType<SolutionLoader>().AsImplementedInterfaces();
        builder.RegisterType<ProjectDataExporter>().As<IProjectDataExporter>();
        builder.RegisterType<DockerfileGenerator>().As<IDockerfileGenerator>();
        builder.RegisterType<AspNetDockerImageVersionSelector>().As<IAspNetDockerImageVersionSelector>();
        builder.RegisterType<DotNetSdkImageVersionSelector>().As<IDotNetSdkImageVersionSelector>();
        builder.RegisterType<ProjectDependenciesExporter>().As<IProjectDependenciesExporter>();
        builder.RegisterType<RecentlyOpenedSolutionsService>().As<IRecentlyOpenedSolutionsService>();
        builder.RegisterType<NuGetConfigExtractor>().As<INuGetConfigExtractor>();
        builder.RegisterType<DockerfileTemplateService>();
    }

    private void ConfigureLogging(ContainerBuilder builder)
    {
        var loggerConfiguration = new LoggerConfiguration()
            .WriteTo.File("logs/app.log", rollingInterval: RollingInterval.Day, fileSizeLimitBytes: 3145728);

        ILogger log = loggerConfiguration.CreateLogger();
        Log.Logger = log;
        builder.RegisterInstance<ILogger>(log).SingleInstance();
    }

    private async void RestartApp(object recipient, NeedAppRestartEvent message)
    {
        var oldMainWindow = _mainWindow;

        // Resolve and show main window
        Log.Information("Restaring application");
        var mainWindow = _serviceContainer.Resolve<MainWindow>();
        _mainWindow = mainWindow;
        // Because app closes on last window close - we need to show new window earlier than we close old
        mainWindow.Show();
        oldMainWindow?.Close();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = mainWindow;
        }

    }
}