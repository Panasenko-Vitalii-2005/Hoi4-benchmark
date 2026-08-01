using System.Collections.ObjectModel;
using HOI4Benchmark.App.Commands;
using HOI4Benchmark.Application.Abstractions;
using DomainBenchmarkSession =
    HOI4Benchmark.Domain.Benchmarks.BenchmarkSession;
using HOI4Benchmark.App.Services;
namespace HOI4Benchmark.App.ViewModels;

public sealed class BenchmarkViewModel : ViewModelBase
{
    private readonly IBenchmarkSessionService _sessionService;

    private Guid? _activeSessionId;
    private BenchmarkStatus _status = BenchmarkStatus.Idle;
    private string _autosavePath = string.Empty;
    private string _currentGameDate = "—";
    private string _elapsedTime = "00:00:00";
    private string _statusMessage = "Ready to start benchmark.";
    private int _measurementCount;
    private bool _isBusy;

    public BenchmarkViewModel(
        IBenchmarkSessionService sessionService)
    {
        _sessionService = sessionService
            ?? throw new ArgumentNullException(
                nameof(sessionService));

        StartCommand = new RelayCommand(
            StartBenchmark,
            CanStartBenchmark);

        StopCommand = new RelayCommand(
            StopBenchmark,
            CanStopBenchmark);

        ResetCommand = new RelayCommand(
            Reset,
            CanReset);
    }

    public string Title => "Benchmark";

    public string Subtitle =>
        "Measure Hearts of Iron IV simulation performance";

    public ObservableCollection<BenchmarkMeasurementItem>
        Measurements { get; } = [];

    public string AutosavePath
    {
        get => _autosavePath;
        set => SetProperty(ref _autosavePath, value);
    }

    public string CurrentGameDate
    {
        get => _currentGameDate;
        private set =>
            SetProperty(ref _currentGameDate, value);
    }

    public string ElapsedTime
    {
        get => _elapsedTime;
        private set =>
            SetProperty(ref _elapsedTime, value);
    }

    public int MeasurementCount
    {
        get => _measurementCount;
        private set =>
            SetProperty(ref _measurementCount, value);
    }

    public BenchmarkStatus Status
    {
        get => _status;
        private set
        {
            if (SetProperty(ref _status, value))
            {
                OnPropertyChanged(nameof(StatusText));
                RaiseCommandStates();
            }
        }
    }

    public string StatusText => Status switch
    {
        BenchmarkStatus.Idle => "Idle",
        BenchmarkStatus.WaitingForAutosave =>
            "Waiting for autosave",
        BenchmarkStatus.Running => "Running",
        BenchmarkStatus.Completed => "Completed",
        BenchmarkStatus.Cancelled => "Cancelled",
        BenchmarkStatus.Failed => "Failed",
        _ => "Unknown"
    };

    public string StatusMessage
    {
        get => _statusMessage;
        private set =>
            SetProperty(ref _statusMessage, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (SetProperty(ref _isBusy, value))
            {
                RaiseCommandStates();
            }
        }
    }

    public RelayCommand StartCommand { get; }

    public RelayCommand StopCommand { get; }

    public RelayCommand ResetCommand { get; }

    private bool CanStartBenchmark()
    {
        return !IsBusy
               && Status is
                   BenchmarkStatus.Idle or
                   BenchmarkStatus.Completed or
                   BenchmarkStatus.Cancelled or
                   BenchmarkStatus.Failed;
    }

    private bool CanStopBenchmark()
    {
        return !IsBusy
               && _activeSessionId.HasValue
               && Status is
                   BenchmarkStatus.WaitingForAutosave or
                   BenchmarkStatus.Running;
    }

    private bool CanReset()
    {
        return !IsBusy
               && Status is not
                   BenchmarkStatus.WaitingForAutosave
               && Status is not
                   BenchmarkStatus.Running;
    }

    private async void StartBenchmark()
    {
        try
        {
            IsBusy = true;
            StatusMessage = "Starting benchmark session...";

            string sessionName =
                $"HOI4 benchmark {DateTime.Now:yyyy-MM-dd HH:mm:ss}";

            DomainBenchmarkSession session =
                await _sessionService.StartBenchmarkAsync(
                    sessionName,
                    targetMeasuredMonths: 12,
                    warmupMonths: 0);

            _activeSessionId = session.Id;

            Measurements.Clear();
            MeasurementCount = 0;
            ElapsedTime = "00:00:00";
            CurrentGameDate = "Waiting for autosave...";

            Status = BenchmarkStatus.WaitingForAutosave;

            StatusMessage =
                $"Session started: {session.Name}";
        }
        catch (Exception exception)
{
    Status = BenchmarkStatus.Failed;

    StatusMessage =
        ErrorMessageProvider.GetMessage(
            "Could not start the benchmark",
            exception);
}
        finally
        {
            IsBusy = false;
        }
    }

    private async void StopBenchmark()
    {
        if (!_activeSessionId.HasValue)
        {
            return;
        }

        try
        {
            IsBusy = true;
            StatusMessage = "Stopping benchmark session...";

            DomainBenchmarkSession session =
                await _sessionService.StopBenchmarkAsync(
                    _activeSessionId.Value);

            _activeSessionId = null;

            Status = BenchmarkStatus.Cancelled;
            CurrentGameDate = "—";

            StatusMessage =
                $"Session stopped with status: {session.Status}.";
        }
        catch (Exception exception)
{
    Status = BenchmarkStatus.Failed;

    StatusMessage =
        ErrorMessageProvider.GetMessage(
            "Could not stop the benchmark",
            exception);
}
        finally
        {
            IsBusy = false;
        }
    }

    private void Reset()
    {
        _activeSessionId = null;

        Measurements.Clear();

        MeasurementCount = 0;
        ElapsedTime = "00:00:00";
        CurrentGameDate = "—";
        Status = BenchmarkStatus.Idle;
        StatusMessage = "Ready to start benchmark.";
    }

    private async void RestoreCurrentSession()
    {
        try
        {
            DomainBenchmarkSession? session =
                await _sessionService.GetCurrentSessionAsync();

            if (session is null)
            {
                return;
            }

            _activeSessionId = session.Id;
            MeasurementCount = session.MeasuredMonthCount;

            Status = session.Status switch
            {
                HOI4Benchmark.Domain.Benchmarks.BenchmarkStatus
                    .WaitingForAutosave =>
                    BenchmarkStatus.WaitingForAutosave,

                HOI4Benchmark.Domain.Benchmarks.BenchmarkStatus
                    .Running =>
                    BenchmarkStatus.Running,

                _ => BenchmarkStatus.Idle
            };

            CurrentGameDate =
                Status == BenchmarkStatus.WaitingForAutosave
                    ? "Waiting for autosave..."
                    : "Measurement in progress";

            StatusMessage =
                $"Restored active session: {session.Name}";
        }
        catch (Exception exception)
{
    Status = BenchmarkStatus.Failed;

    StatusMessage =
        ErrorMessageProvider.GetMessage(
            "Could not restore the active benchmark",
            exception);
}
    }

    private void RaiseCommandStates()
    {
        StartCommand.RaiseCanExecuteChanged();
        StopCommand.RaiseCanExecuteChanged();
        ResetCommand.RaiseCanExecuteChanged();
    }
    
}