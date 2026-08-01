using System.Windows.Input;
using HOI4Benchmark.App.Commands;
using HOI4Benchmark.App.Navigation;
using HOI4Benchmark.Application.Abstractions;

namespace HOI4Benchmark.App.ViewModels;

public sealed class DashboardViewModel : ViewModelBase
{
    private readonly ISystemInformationProvider
        _systemInformationProvider;

    private bool _isLoading;
    private string _status = "Ready";
    private string _processorName = "Not loaded";
    private string _graphicsAdapter = "Not loaded";
    private string _installedMemory = "Not loaded";
    private string _operatingSystem = "Not loaded";

    public DashboardViewModel(
        INavigationService navigationService,
        ISystemInformationProvider
            systemInformationProvider)
    {
        ArgumentNullException.ThrowIfNull(
            navigationService);

        _systemInformationProvider =
            systemInformationProvider;

        StartBenchmarkCommand =
            new RelayCommand(
                navigationService.NavigateToBenchmark);

        RefreshCommand =
            new RelayCommand(
                async () => await LoadAsync(),
                () => !IsLoading);
    }

    public string Title => "Dashboard";

    public string Subtitle =>
        "System overview and recent benchmark activity";

    public int TotalResults { get; private set; }

    public string BestScore { get; private set; } = "—";

    public string LastBenchmark { get; private set; } = "—";

    public bool IsLoading
    {
        get => _isLoading;
        private set
        {
            if (SetProperty(ref _isLoading, value))
            {
                RefreshCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string Status
    {
        get => _status;
        private set => SetProperty(ref _status, value);
    }

    public string ProcessorName
    {
        get => _processorName;
        private set =>
            SetProperty(ref _processorName, value);
    }

    public string GraphicsAdapter
    {
        get => _graphicsAdapter;
        private set =>
            SetProperty(ref _graphicsAdapter, value);
    }

    public string InstalledMemory
    {
        get => _installedMemory;
        private set =>
            SetProperty(ref _installedMemory, value);
    }

    public string OperatingSystem
    {
        get => _operatingSystem;
        private set =>
            SetProperty(ref _operatingSystem, value);
    }

    public ICommand StartBenchmarkCommand { get; }

    public RelayCommand RefreshCommand { get; }

    public async Task LoadAsync()
    {
        if (IsLoading)
        {
            return;
        }

        IsLoading = true;
        Status = "Loading system information...";

        try
        {

            var systemInformation =
                await _systemInformationProvider
                    .GetSystemInformationAsync();

            ProcessorName =
                systemInformation.ProcessorName;

            GraphicsAdapter =
                systemInformation.GraphicsAdapter;

            InstalledMemory =
                $"{systemInformation.InstalledMemoryMb:N0} MB";

            OperatingSystem =
                systemInformation.OperatingSystem;

            Status = "Ready";
        }
        catch (Exception exception)
        {
            Status =
                $"Failed to load system information: " +
                exception.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }
}