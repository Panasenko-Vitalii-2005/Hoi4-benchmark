using System.Collections.ObjectModel;
using HOI4Benchmark.Domain.Statistics;
using HOI4Benchmark.App.Commands;
using HOI4Benchmark.Application.Abstractions;
using DomainBenchmarkSession =
    HOI4Benchmark.Domain.Benchmarks.BenchmarkSession;
using HOI4Benchmark.App.Services;
using System.IO;
using HOI4Benchmark.Domain.Game;
using HOI4Benchmark.Domain.Settings;
using HOI4Benchmark.Domain.Benchmarks;
namespace HOI4Benchmark.App.ViewModels;

public sealed class BenchmarkViewModel : ViewModelBase
{
    private readonly IBenchmarkSessionService _sessionService;
    private readonly IAutosaveWatcher _autosaveWatcher;
    private readonly IInitialSaveDateParser _saveDateParser;
    private readonly ISettingsService _settingsService;

    private readonly IResultService _resultService;

    private DomainBenchmarkSession? _activeSession;
private GameDate? _previousGameDate;
private DateTimeOffset? _previousAutosaveDetectedAtUtc;
private TimeSpan _totalMeasuredTime = TimeSpan.Zero;
    private Guid? _activeSessionId;
    private BenchmarkStatus _status = BenchmarkStatus.Idle;
    private string _autosavePath = string.Empty;
    private string _currentGameDate = "—";
    private string _elapsedTime = "00:00:00";
    private string _statusMessage = "Ready to start benchmark.";
    private int _measurementCount;
    private bool _isBusy;

    public BenchmarkViewModel(
    IBenchmarkSessionService sessionService,
    IAutosaveWatcher autosaveWatcher,
    IInitialSaveDateParser saveDateParser,
    ISettingsService settingsService,
    IResultService resultService)
{
    _sessionService = sessionService
        ?? throw new ArgumentNullException(
            nameof(sessionService));

    _autosaveWatcher = autosaveWatcher
        ?? throw new ArgumentNullException(
            nameof(autosaveWatcher));

    _saveDateParser = saveDateParser
        ?? throw new ArgumentNullException(
            nameof(saveDateParser));

        _settingsService = settingsService
            ?? throw new ArgumentNullException(
                nameof(settingsService));
            
    _resultService = resultService
    ?? throw new ArgumentNullException(
        nameof(resultService));

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

    private async Task OnAutosaveReadyAsync(
    string autosavePath,
    CancellationToken cancellationToken)
{
    GameDate gameDate =
        await _saveDateParser.ParseAsync(
            autosavePath,
            cancellationToken);

    DateTimeOffset detectedAtUtc =
        DateTimeOffset.UtcNow;

    // Первый автосейв — только точка отсчёта.
    if (!_previousGameDate.HasValue ||
        !_previousAutosaveDetectedAtUtc.HasValue)
    {
        _previousGameDate = gameDate;
        _previousAutosaveDetectedAtUtc =
            detectedAtUtc;

        await System.Windows.Application.Current.Dispatcher
            .InvokeAsync(() =>
            {
                CurrentGameDate =
                    gameDate.ToString();

                Status =
                    BenchmarkStatus.Running;

                StatusMessage =
                    $"Initial autosave detected: {gameDate}. " +
                    "Waiting for the next monthly autosave.";
            });

        return;
    }

    // FileSystemWatcher иногда может несколько раз
    // сообщить об одном и том же автосейве.
    if (gameDate == _previousGameDate.Value)
    {
        return;
    }

    GameDate fromDate =
        _previousGameDate.Value;

    DateTimeOffset measurementStartedAtUtc =
        _previousAutosaveDetectedAtUtc.Value;

    TimeSpan elapsedTime =
        detectedAtUtc -
        measurementStartedAtUtc;

    if (elapsedTime <= TimeSpan.Zero)
    {
        return;
    }

    DomainBenchmarkSession? session =
        _activeSession;

    if (session is null)
    {
        return;
    }

    int measurementIndex =
        session.Measurements.Count + 1;

    bool isWarmup =
        measurementIndex <= session.WarmupMonths;

    IReadOnlyList<string> warnings =
        gameDate.IsNextMonthAfter(fromDate)
            ? []
            :
            [
                $"Unexpected date transition: " +
                $"{fromDate} → {gameDate}."
            ];

    var measurement =
        new MonthlyMeasurement(
            measurementIndex,
            fromDate,
            gameDate,
            elapsedTime,
            measurementStartedAtUtc,
            detectedAtUtc,
            isWarmup,
            warnings);

    session.AddMeasurement(measurement);

    _previousGameDate = gameDate;
    _previousAutosaveDetectedAtUtc =
        detectedAtUtc;

    _totalMeasuredTime += elapsedTime;

    await System.Windows.Application.Current.Dispatcher
        .InvokeAsync(() =>
        {
            CurrentGameDate =
                gameDate.ToString();

            Status =
                BenchmarkStatus.Running;

            Measurements.Add(
                new BenchmarkMeasurementItem
                {
                    GameDate =
                        $"{fromDate} → {gameDate}",

                    Duration =
                        FormatDuration(elapsedTime),

                    Score =
                        isWarmup
                            ? "Warm-up"
                            : "Measured"
                });

            MeasurementCount =
                session.MeasuredMonthCount;

            ElapsedTime =
                FormatTotalDuration(
                    _totalMeasuredTime);

            StatusMessage =
                isWarmup
                    ? $"Warm-up month {measurementIndex} recorded."
                    : $"Measurement {session.MeasuredMonthCount} " +
                      $"recorded: {FormatDuration(elapsedTime)}.";
        });

    if (session.MeasuredMonthCount <
        session.TargetMeasuredMonths)
    {
        return;
    }

    BenchmarkResult result =
        session.Complete(
            DateTimeOffset.UtcNow);

    await _resultService.SaveResultAsync(
        result);

    await _autosaveWatcher.StopAsync();

    _activeSessionId = null;
    _activeSession = null;

    _previousGameDate = null;
    _previousAutosaveDetectedAtUtc = null;

    await System.Windows.Application.Current.Dispatcher
        .InvokeAsync(() =>
        {
            Status =
                BenchmarkStatus.Completed;

            StatusMessage =
                $"Benchmark completed and saved. " +
                $"{result.Measurements.Count} measurements recorded.";

            RaiseCommandStates();
        });
}

    private static string FormatDuration(
        TimeSpan duration)
    {
        return duration.TotalSeconds.ToString(
            "0.000",
            System.Globalization.CultureInfo.InvariantCulture)
            + " s";
    }

    private static string FormatTotalDuration(
        TimeSpan duration)
    {
        return duration.ToString(
            @"hh\:mm\:ss");
    }

private async Task<BenchmarkResult> SavePartialResultAsync(
    DomainBenchmarkSession session)
{
    if (session.Measurements.Count == 0)
    {
        throw new InvalidOperationException(
            "A partial benchmark result requires at least one measurement.");
    }

    var statisticsCalculator =
        new StatisticsCalculator();

    BenchmarkStatistics statistics =
        statisticsCalculator.Calculate(
            session.Measurements);

    var result =
        new BenchmarkResult(
            session.Id,
            $"{session.Name} (partial)",
            session.StartedAtUtc,
            DateTimeOffset.UtcNow,
            session.Measurements
                .OrderBy(measurement => measurement.Index)
                .ToArray(),
            statistics);

    await _resultService.SaveResultAsync(result);

    return result;
}

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

        BenchmarkSettings settings =
            await _settingsService.GetSettingsAsync();

        if (string.IsNullOrWhiteSpace(
                settings.SavePath))
        {
            throw new DirectoryNotFoundException(
                "Autosave directory is not configured.");
        }

        string autosaveDirectory =
            Path.GetFullPath(
                settings.SavePath.Trim());

        if (!Directory.Exists(
                autosaveDirectory))
        {
            throw new DirectoryNotFoundException(
                "The configured autosave directory was not found.");
        }

        string autosavePath =
            Path.Combine(
                autosaveDirectory,
                "autosave_temp.hoi4");

        if (_autosaveWatcher.IsRunning)
        {
            await _autosaveWatcher.StopAsync();
        }

        string sessionName =
            $"HOI4 benchmark {DateTime.Now:yyyy-MM-dd HH:mm:ss}";

        DomainBenchmarkSession session =
            await _sessionService.StartBenchmarkAsync(
                sessionName,
                targetMeasuredMonths:
                    settings.TargetMeasuredMonths,
                warmupMonths:
                    settings.WarmupMonths);

        _activeSessionId = session.Id;
_activeSession = session;

_previousGameDate = null;
_previousAutosaveDetectedAtUtc = null;
_totalMeasuredTime = TimeSpan.Zero;

Measurements.Clear();
        MeasurementCount = 0;
        ElapsedTime = "00:00:00";
        CurrentGameDate = "Waiting for autosave...";

        Status =
            BenchmarkStatus.WaitingForAutosave;

        await _autosaveWatcher.StartAsync(
            autosavePath,
            OnAutosaveReadyAsync);

        StatusMessage =
            $"Watching autosave file: {autosavePath}";
    }
    catch (Exception exception)
    {
        if (_autosaveWatcher.IsRunning)
        {
            await _autosaveWatcher.StopAsync();
        }

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

            await _autosaveWatcher.StopAsync();

DomainBenchmarkSession? activeSession =
    _activeSession;

BenchmarkResult? partialResult = null;

if (activeSession is not null &&
    activeSession.Measurements.Count > 0)
{
    partialResult =
        await SavePartialResultAsync(
            activeSession);
}

DomainBenchmarkSession session =
    await _sessionService.StopBenchmarkAsync(
        _activeSessionId.Value);

_activeSessionId = null;
_activeSession = null;

_previousGameDate = null;
_previousAutosaveDetectedAtUtc = null;
_totalMeasuredTime = TimeSpan.Zero;

Status = BenchmarkStatus.Cancelled;
            CurrentGameDate = "—";

            StatusMessage =
    partialResult is not null
        ? $"Benchmark stopped. Partial result saved with " +
          $"{partialResult.Measurements.Count} measurements."
        : $"Session stopped with status: {session.Status}. " +
          "No measurements were available to save.";
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
    _activeSession = null;

    _previousGameDate = null;
    _previousAutosaveDetectedAtUtc = null;
    _totalMeasuredTime = TimeSpan.Zero;

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