using System.Collections.ObjectModel;
using System.IO;
using HOI4Benchmark.App.Commands;
using HOI4Benchmark.App.Services;
using HOI4Benchmark.Application.Abstractions;
using HOI4Benchmark.Domain.Benchmarks;
using Microsoft.Win32;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using HOI4Benchmark.Application.Export;

namespace HOI4Benchmark.App.ViewModels;

public sealed class ResultsViewModel : ViewModelBase
{
    public RelayCommand ExportAllJsonCommand { get; }
    public RelayCommand ExportAllCsvCommand { get; }
    private ISeries[] _monthlySeries = [];
    private Axis[] _monthlyXAxes = [];
    private Axis[] _monthlyYAxes = [];

    public ISeries[] MonthlySeries
    {
        get => _monthlySeries;
        private set => SetProperty(ref _monthlySeries, value);
    }

    public Axis[] MonthlyXAxes
    {
        get => _monthlyXAxes;
        private set => SetProperty(ref _monthlyXAxes, value);
    }

    public Axis[] MonthlyYAxes
    {
        get => _monthlyYAxes;
        private set => SetProperty(ref _monthlyYAxes, value);
    }

    private readonly IResultService _resultService;
    private readonly IExportService _exportService;

    private BenchmarkResultItem? _selectedResult;
    private string _statusMessage =
        "Loading benchmark results...";
    private bool _isBusy;

    private bool _anonymizeResultNames = true;
private bool _excludeExactTimestamps = true;
private bool _excludeWarnings = true;

    public ResultsViewModel(
        IResultService resultService,
        IExportService exportService)
    {
        _resultService = resultService
            ?? throw new ArgumentNullException(
                nameof(resultService));

        _exportService = exportService
            ?? throw new ArgumentNullException(
                nameof(exportService));

        RefreshCommand = new RelayCommand(
            Refresh,
            () => !IsBusy);

        ExportJsonCommand = new RelayCommand(
            ExportJson,
            CanUseSelectedResult);

        ExportCsvCommand = new RelayCommand(
            ExportCsv,
            CanUseSelectedResult);

        ExportAllJsonCommand = new RelayCommand(
    ExportAllJson,
    () => !IsBusy && Results.Count > 0);

ExportAllCsvCommand = new RelayCommand(
    ExportAllCsv,
    () => !IsBusy && Results.Count > 0);

        DeleteCommand = new RelayCommand(
            DeleteSelected,
            CanUseSelectedResult);


        Refresh();
    }

    public string Title => "Results";

    public string Subtitle =>
        "Review, compare and export completed benchmark sessions";

    public ObservableCollection<BenchmarkResultItem>
        Results
    { get; } = [];

    public BenchmarkResultItem? SelectedResult
    {
        get => _selectedResult;
        set
        {
            if (!SetProperty(ref _selectedResult, value))
            {
                return;
            }
            OnPropertyChanged(nameof(HasSelectedResult));
            BuildMonthlyChart();
            RaiseCommandStates();
        }
    }

    public bool HasSelectedResult =>
        SelectedResult is not null;
    
    public bool AnonymizeResultNames
{
    get => _anonymizeResultNames;
    set => SetProperty(
        ref _anonymizeResultNames,
        value);
}

public bool ExcludeExactTimestamps
{
    get => _excludeExactTimestamps;
    set => SetProperty(
        ref _excludeExactTimestamps,
        value);
}

public bool ExcludeWarnings
{
    get => _excludeWarnings;
    set => SetProperty(
        ref _excludeWarnings,
        value);
}

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

    public RelayCommand RefreshCommand { get; }

    public RelayCommand ExportJsonCommand { get; }

    public RelayCommand ExportCsvCommand { get; }

    public RelayCommand DeleteCommand { get; }

    private bool CanUseSelectedResult()
    {
        return !IsBusy && SelectedResult is not null;
    }

    private async void Refresh()
    {
        try
        {
            IsBusy = true;
            StatusMessage = "Loading benchmark results...";

            IEnumerable<BenchmarkResult> storedResults =
                await _resultService.GetAllResultsAsync();

            BenchmarkResultItem[] items = storedResults
                .OrderByDescending(result =>
                    result.CompletedAtUtc)
                .Select(result =>
                    new BenchmarkResultItem(result))
                .ToArray();

            Results.Clear();

            foreach (BenchmarkResultItem item in items)
            {
                Results.Add(item);
            }

            SelectedResult = Results.FirstOrDefault();

            StatusMessage = Results.Count == 0
                ? "No benchmark results found."
                : $"{Results.Count} benchmark result(s) loaded.";
        }
        catch (Exception exception)
{
    StatusMessage =
        ErrorMessageProvider.GetMessage(
            "Could not load benchmark results",
            exception);
}
        finally
        {
            IsBusy = false;
        }
    }

    private ExportPrivacyOptions CreatePrivacyOptions()
{
    return new ExportPrivacyOptions
    {
        AnonymizeResultNames =
            AnonymizeResultNames,

        ExcludeExactTimestamps =
            ExcludeExactTimestamps,

        ExcludeWarnings =
            ExcludeWarnings
    };
}

    private async void ExportJson()
    {
        BenchmarkResultItem? selected =
            SelectedResult;

        if (selected is null)
        {
            return;
        }

        try
        {
            IsBusy = true;

            string content =
    await _exportService.ExportDetailedResultToJsonAsync(
        selected.Source,
        CreatePrivacyOptions());

            bool saved = SaveExportFile(
    content,
    AnonymizeResultNames
        ? "hoi4-benchmark-result.json"
        : $"{selected.Source.Name}.json",
    "JSON files (*.json)|*.json");

StatusMessage = saved
    ? "The selected result was exported to JSON."
    : "JSON export was cancelled.";
        }
        catch (Exception exception)
{
    StatusMessage =
        ErrorMessageProvider.GetMessage(
            "JSON export failed",
            exception);
}
        finally
        {
            IsBusy = false;
        }
    }

    private async void ExportCsv()
    {
        BenchmarkResultItem? selected =
            SelectedResult;

        if (selected is null)
        {
            return;
        }

        try
        {
            IsBusy = true;

            string content =
    await _exportService.ExportMeasurementsToCsvAsync(
        selected.Source,
        CreatePrivacyOptions());

            bool saved = SaveExportFile(
    content,
    AnonymizeResultNames
        ? "hoi4-benchmark-measurements.csv"
        : $"{selected.Source.Name}.csv",
    "CSV files (*.csv)|*.csv");

StatusMessage = saved
    ? "The selected result was exported to CSV."
    : "CSV export was cancelled.";
        }
        catch (Exception exception)
{
    StatusMessage =
        ErrorMessageProvider.GetMessage(
            "CSV export failed",
            exception);
}
        finally
        {
            IsBusy = false;
        }
    }

    private async void DeleteSelected()
    {
        BenchmarkResultItem? selected =
            SelectedResult;

        if (selected is null)
        {
            return;
        }

        try
        {
            IsBusy = true;

            bool deleted =
                await _resultService.DeleteResultAsync(
                    selected.Id.ToString());

            if (!deleted)
            {
                StatusMessage =
                    "The selected benchmark result was not found.";

                return;
            }

            Results.Remove(selected);
            SelectedResult = Results.FirstOrDefault();

            StatusMessage =
                $"Benchmark result deleted: {selected.Name}.";
        }
        catch (Exception exception)
{
    StatusMessage =
        ErrorMessageProvider.GetMessage(
            "Could not delete the benchmark result",
            exception);
}
        finally
        {
            IsBusy = false;
        }
    }

    private static bool SaveExportFile(
    string content,
    string suggestedName,
    string filter)
    {
        string safeName = string.Concat(
            suggestedName.Select(character =>
                Path.GetInvalidFileNameChars()
                    .Contains(character)
                    ? '_'
                    : character));

        var dialog = new SaveFileDialog
        {
            FileName = safeName,
            Filter = filter,
            AddExtension = true,
            OverwritePrompt = true
        };

        if (dialog.ShowDialog() != true)
        {
            return false;
        }

        File.WriteAllText(
            dialog.FileName,
            content);

        return true;
    }

    private void RaiseCommandStates()
    {
        RefreshCommand.RaiseCanExecuteChanged();
    ExportJsonCommand.RaiseCanExecuteChanged();
    ExportCsvCommand.RaiseCanExecuteChanged();
    ExportAllJsonCommand.RaiseCanExecuteChanged();
    ExportAllCsvCommand.RaiseCanExecuteChanged();
    DeleteCommand.RaiseCanExecuteChanged();
    }

    private void BuildMonthlyChart()
    {
        BenchmarkResultItem? selected = SelectedResult;

        if (selected is null)
        {
            MonthlySeries = [];
            MonthlyXAxes = [];
            MonthlyYAxes = [];

            return;
        }

        var measurements = selected.Source.Measurements
            .OrderBy(measurement => measurement.Index)
            .ToArray();

        if (measurements.Length == 0)
        {
            MonthlySeries = [];
            MonthlyXAxes = [];
            MonthlyYAxes = [];

            return;
        }

        double[] values = measurements
            .Select(measurement =>
                measurement.ElapsedTime.TotalSeconds)
            .ToArray();

        double maximumValue =
    values.Max();

double yAxisMaximum =
    maximumValue <= 0
        ? 1
        : maximumValue * 1.15;

        int labelStep =
    measurements.Length switch
    {
        <= 18 => 1,
        <= 36 => 2,
        <= 72 => 3,
        _ => 6
    };

string[] labels = measurements
    .Select((measurement, index) =>
        index % labelStep == 0
            ? $"{measurement.ToDate.Year:D4}-" +
              $"{measurement.ToDate.Month:D2}"
            : string.Empty)
    .ToArray();

        var textPaint = new SolidColorPaint(
    new SKColor(31, 41, 55));

var secondaryTextPaint = new SolidColorPaint(
    new SKColor(75, 85, 99));

var separatorPaint = new SolidColorPaint(
    new SKColor(229, 231, 235))
{
    StrokeThickness = 1
};

        SolidColorPaint pointFill =
    new(new SKColor(59, 130, 246));

SolidColorPaint pointStroke =
    new(new SKColor(37, 99, 235))
    {
        StrokeThickness = 2
    };

if (values.Length == 1)
{
    MonthlySeries =
    [
        new ScatterSeries<double>
        {
            Name = "Month time",
            Values = values,
            GeometrySize = 16,
            Fill = pointFill,
            Stroke = pointStroke,

            YToolTipLabelFormatter = point =>
                $"{point.Coordinate.PrimaryValue:0.###} s"
        }
    ];
}
else
{
    MonthlySeries =
    [
        new LineSeries<double>
        {
            Name = "Month time",
            Values = values,
            Fill = null,
            GeometrySize = 10,
            LineSmoothness = 0,

            Stroke = new SolidColorPaint(
                new SKColor(59, 130, 246))
            {
                StrokeThickness = 3
            },

            GeometryFill = pointFill,
            GeometryStroke = pointStroke,

            YToolTipLabelFormatter = point =>
                $"{point.Coordinate.PrimaryValue:0.###} s"
        }
    ];
}
        MonthlyXAxes =
[
    new Axis
    {
        Name = "Game month",
        Labels = labels,
        LabelsRotation = 0,
        TextSize = 11,
        LabelsPaint = secondaryTextPaint,
        NamePaint = textPaint,
        SeparatorsPaint = separatorPaint,
        MinStep = 1,
        ForceStepToMin = true,

        // Не даём единственной точке оказаться
        // прямо на краю области построения.
        MinLimit = -0.5,
        MaxLimit = measurements.Length - 0.5
    }
];

        MonthlyYAxes =
[
    new Axis
    {
        Name = "Duration, seconds",
        TextSize = 12,
        LabelsPaint = secondaryTextPaint,
        NamePaint = textPaint,
        SeparatorsPaint = separatorPaint,
        Labeler = value => $"{value:0.##} s",
        MinLimit = 0,
        MaxLimit = yAxisMaximum
    }
];
    }

private async void ExportAllJson()
{
    try
    {
        IsBusy = true;

        BenchmarkResult[] results =
            Results
                .Select(item => item.Source)
                .ToArray();

        string content =
    await _exportService
        .ExportDetailedResultsToJsonAsync(
            results,
            CreatePrivacyOptions());

        bool saved = SaveExportFile(
            content,
            $"hoi4-benchmark-results-" +
            $"{DateTime.Now:yyyy-MM-dd}.json",
            "JSON files (*.json)|*.json");

        StatusMessage = saved
            ? $"{results.Length} result(s) exported to JSON."
            : "JSON export cancelled.";
    }
    catch (Exception exception)
{
    StatusMessage =
        ErrorMessageProvider.GetMessage(
            "Could not export all results to JSON",
            exception);
}
    finally
    {
        IsBusy = false;
    }
}

private async void ExportAllCsv()
{
    try
    {
        IsBusy = true;

        BenchmarkResult[] results =
            Results
                .Select(item => item.Source)
                .ToArray();

        string content =
    await _exportService
        .ExportMeasurementsToCsvAsync(
            results,
            CreatePrivacyOptions());

        bool saved = SaveExportFile(
            content,
            $"hoi4-benchmark-measurements-" +
            $"{DateTime.Now:yyyy-MM-dd}.csv",
            "CSV files (*.csv)|*.csv");

        StatusMessage = saved
            ? $"{results.Length} result(s) exported to CSV."
            : "CSV export cancelled.";
    }
    catch (Exception exception)
{
    StatusMessage =
        ErrorMessageProvider.GetMessage(
            "Could not export all results to CSV",
            exception);
}
    finally
    {
        IsBusy = false;
    }
}

}