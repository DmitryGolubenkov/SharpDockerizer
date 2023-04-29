using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.Messaging;
using SharpDockerizer.AppLayer.Contracts;
using SharpDockerizer.AppLayer.Generation;
using SharpDockerizer.AppLayer.Services.Project;
using SharpDockerizer.AppLayer.Services.Solution;
using Serilog;
using Autofac;
using SharpDockerizer.AvaloniaUI.Services;
using SharpDockerizer.AvaloniaUI.Services.Localization;

namespace SharpDockerizer.AvaloniaUI;
public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var builder = new ContainerBuilder();

        ConfigureServices(builder);
        var services = builder.Build();

        // Used in DI and MVVM
        DISource.Resolver = services.Resolve;

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();
        }

        Log.Information("Application started");

        base.OnFrameworkInitializationCompleted();
    }

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

        builder.RegisterType<OptionsService>().AsSelf().SingleInstance();
        builder.RegisterType<LocalizationController>().AsSelf().SingleInstance();

        // Register other services required for application to function
        builder.RegisterType<CurrentSolutionInfo>().As<ICurrentSolutionInfo>().SingleInstance();
        builder.RegisterType<SolutionLoader>().AsImplementedInterfaces();
        builder.RegisterType<ProjectDataExporter>().As<IProjectDataExporter>();
        builder.RegisterType<StandartDockerfileGenerator>().As<IDockerfileGenerator>();
        builder.RegisterType<AspNetDockerImageVersionSelector>().As<IAspNetDockerImageVersionSelector>();
        builder.RegisterType<DotNetSdkImageVersionSelector>().As<IDotNetSdkImageVersionSelector>();
        builder.RegisterType<ProjectDependenciesExporter>().As<IProjectDependenciesExporter>();
        builder.RegisterType<RecentlyOpenedSolutionsService>().As<IRecentlyOpenedSolutionsService>();
        builder.RegisterType<NuGetConfigExtractor>().As<INuGetConfigExtractor>();
    }

   // private void ConfigureLogging(IServiceCollection services)
    private void ConfigureLogging(ContainerBuilder builder)
    {
        var loggerConfiguration = new LoggerConfiguration()
           .WriteTo.File("logs/app.log", rollingInterval: RollingInterval.Day, fileSizeLimitBytes: 3145728);

        ILogger log = loggerConfiguration.CreateLogger();
        Log.Logger = log;
        builder.RegisterInstance<ILogger>(log).SingleInstance();
    }
}
