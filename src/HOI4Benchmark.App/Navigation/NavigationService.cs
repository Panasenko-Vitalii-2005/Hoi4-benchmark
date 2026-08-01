using System.Windows;
using System.Windows.Controls;
using HOI4Benchmark.App.Views;
using Microsoft.Extensions.DependencyInjection;

namespace HOI4Benchmark.App.Navigation;

public sealed class NavigationService : INavigationService
{
    private readonly IServiceProvider _serviceProvider;

    private UserControl? _currentView;

    public NavigationService(
        IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider
            ?? throw new ArgumentNullException(
                nameof(serviceProvider));
    }

    public UserControl? CurrentView
    {
        get => _currentView;
        private set
        {
            if (ReferenceEquals(_currentView, value))
            {
                return;
            }

            _currentView = value;
            CurrentViewChanged?.Invoke(
                this,
                EventArgs.Empty);
        }
    }

    public event EventHandler? CurrentViewChanged;

    public void NavigateToDashboard()
    {
        NavigateTo<DashboardView>();
    }

    public void NavigateToBenchmark()
    {
        NavigateTo<BenchmarkView>();
    }

    public void NavigateToCompareResults()
{
    NavigateTo<CompareResultsView>();
}

    public void NavigateToResults()
    {
        NavigateTo<ResultsView>();
    }

    public void NavigateToSettings()
    {
        NavigateTo<SettingsView>();
    }

    private void NavigateTo<TView>()
        where TView : UserControl
    {
        try
        {
            CurrentView =
                _serviceProvider.GetRequiredService<TView>();
        }
        catch (Exception exception)
        {
            MessageBox.Show(
                exception.ToString(),
                $"Could not open {typeof(TView).Name}",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }
}