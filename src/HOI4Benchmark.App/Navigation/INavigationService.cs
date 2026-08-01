using System.Windows.Controls;

namespace HOI4Benchmark.App.Navigation;

public interface INavigationService
{
    UserControl? CurrentView { get; }

    event EventHandler? CurrentViewChanged;

    void NavigateToDashboard();

    void NavigateToBenchmark();

    void NavigateToResults();

    void NavigateToCompareResults();

    void NavigateToSettings();
}