using System.Windows;
using HOI4Benchmark.App.DependencyInjection;
using HOI4Benchmark.App.Navigation;
using HOI4Benchmark.App.Services;
using HOI4Benchmark.App.ViewModels;
using HOI4Benchmark.App.Views;
using HOI4Benchmark.Application.Abstractions;
using HOI4Benchmark.Application.Implementations;
using HOI4Benchmark.Domain.Benchmarks;
using HOI4Benchmark.Domain.Common;
using HOI4Benchmark.Infrastructure;
using HOI4Benchmark.Infrastructure.Export;
using HOI4Benchmark.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace HOI4Benchmark.App;

public partial class App : System.Windows.Application
{
    private ServiceProvider? _serviceProvider;
    private IAppLogger? _logger;

    protected override void OnStartup(StartupEventArgs e)
    {
        
        DispatcherUnhandledException += (_, args) =>
{
    if (_logger is not null)
{
    _ = _logger.LogErrorAsync(
        args.Exception,
        "Unhandled UI exception.");
}
    MessageBox.Show(
        ErrorMessageProvider.GetMessage(
            args.Exception),
        "Application error",
        MessageBoxButton.OK,
        MessageBoxImage.Error);

    args.Handled = true;
};

        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
{
    if (args.ExceptionObject is Exception fatalException)
{
    if (_logger is not null)
{
    _ = _logger.LogErrorAsync(
        fatalException,
        "Fatal application exception.");
}
}
    string message =
        args.ExceptionObject is Exception exception
            ? ErrorMessageProvider.GetMessage(exception)
            : "A fatal application error occurred.";

    MessageBox.Show(
        message,
        "Fatal application error",
        MessageBoxButton.OK,
        MessageBoxImage.Error);
};
        TaskScheduler.UnobservedTaskException += (_, args) =>
{
    if (_logger is not null)
{
    _ = _logger.LogErrorAsync(
        args.Exception,
        "Unobserved background task exception.");
}
    MessageBox.Show(
        ErrorMessageProvider.GetMessage(
            args.Exception),
        "Background task error",
        MessageBoxButton.OK,
        MessageBoxImage.Warning);

    args.SetObserved();
};

        base.OnStartup(e);

        var services = new ServiceCollection();

        services.AddApplicationServices();
        services.AddInfrastructureServices();

        services.AddSingleton<
            IRepository<BenchmarkSession>,
            Repository<BenchmarkSession>>();

        services.AddTransient<
            IBenchmarkSessionService,
            BenchmarkSessionService>();

        services.AddTransient<
            IResultService,
            ResultService>();

        services.AddTransient<
            ISettingsService,
            SettingsService>();

        services.AddTransient<
            IExportService,
            ExportService>();

        services.AddSingleton<
            INavigationService,
            NavigationService>();

        services.AddSingleton<MainWindow>();
        services.AddSingleton<MainWindowViewModel>();

        services.AddTransient<DashboardView>();
        services.AddSingleton<BenchmarkView>();
        services.AddTransient<ResultsView>();
        services.AddTransient<SettingsView>();

        services.AddTransient<DashboardViewModel>();
        services.AddSingleton<BenchmarkViewModel>();
        services.AddTransient<ResultsViewModel>();
        services.AddTransient<SettingsViewModel>();
        services.AddTransient<CompareResultsViewModel>();
        services.AddTransient<CompareResultsView>();

        _serviceProvider = services.BuildServiceProvider(
            new ServiceProviderOptions
            {
                ValidateOnBuild = true,
                ValidateScopes = true
            });

            _logger =
    _serviceProvider.GetRequiredService<
        IAppLogger>();

_ = _logger.LogInformationAsync(
    "HOI4 Benchmark application started.");

        MainWindow mainWindow =
            _serviceProvider.GetRequiredService<MainWindow>();

        INavigationService navigationService =
            _serviceProvider.GetRequiredService<INavigationService>();

        navigationService.NavigateToDashboard();

        mainWindow.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        if (_logger is not null)
{
    _ = _logger.LogInformationAsync(
        "HOI4 Benchmark application stopped.");
}
        _serviceProvider?.Dispose();

        base.OnExit(e);
    }
}