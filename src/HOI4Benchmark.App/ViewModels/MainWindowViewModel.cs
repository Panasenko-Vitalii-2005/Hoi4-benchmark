using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Input;
using HOI4Benchmark.App.Commands;
using HOI4Benchmark.App.Navigation;

namespace HOI4Benchmark.App.ViewModels;

public sealed class MainWindowViewModel
    : INotifyPropertyChanged
{
    private readonly INavigationService _navigationService;

    public MainWindowViewModel(
        INavigationService navigationService)
    {
        _navigationService = navigationService;

        NavigateDashboardCommand =
            new RelayCommand(
                _navigationService.NavigateToDashboard);

        NavigateBenchmarkCommand =
            new RelayCommand(
                _navigationService.NavigateToBenchmark);

        NavigateResultsCommand =
            new RelayCommand(
                _navigationService.NavigateToResults);

        NavigateSettingsCommand =
            new RelayCommand(
                _navigationService.NavigateToSettings);

        NavigateCompareResultsCommand =
    new RelayCommand(
        _navigationService.NavigateToCompareResults);

        _navigationService.CurrentViewChanged +=
            OnCurrentViewChanged;
    }

    public UserControl? CurrentView =>
        _navigationService.CurrentView;

    public ICommand NavigateDashboardCommand { get; }
    public ICommand NavigateCompareResultsCommand { get; }

    public ICommand NavigateBenchmarkCommand { get; }

    public ICommand NavigateResultsCommand { get; }

    public ICommand NavigateSettingsCommand { get; }

    public event PropertyChangedEventHandler?
        PropertyChanged;

    private void OnCurrentViewChanged(
        object? sender,
        EventArgs e)
    {
        OnPropertyChanged(nameof(CurrentView));
    }

    private void OnPropertyChanged(
        [CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(
            this,
            new PropertyChangedEventArgs(propertyName));
    }
}