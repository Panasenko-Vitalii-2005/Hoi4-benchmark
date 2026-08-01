using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using HOI4Benchmark.App.Services;
using HOI4Benchmark.Application.Abstractions;
using HOI4Benchmark.Domain.Benchmarks;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace HOI4Benchmark.App.ViewModels;

public sealed class CompareResultsViewModel : INotifyPropertyChanged
{
    private ISeries[] _comparisonSeries = [];
private Axis[] _comparisonXAxes = [];
private Axis[] _comparisonYAxes = [];
    private readonly IResultService _resultService;

    private ResultSelectionItem? _selectedFirstResult;
    private ResultSelectionItem? _selectedSecondResult;
    private bool _isLoading;
    private string? _errorMessage;

    public ISeries[] ComparisonSeries
{
    get => _comparisonSeries;
    private set =>
        SetField(ref _comparisonSeries, value);
}

public Axis[] ComparisonXAxes
{
    get => _comparisonXAxes;
    private set =>
        SetField(ref _comparisonXAxes, value);
}

public Axis[] ComparisonYAxes
{
    get => _comparisonYAxes;
    private set =>
        SetField(ref _comparisonYAxes, value);
}

public bool HasChartData =>
    ComparisonSeries.Length > 0;

    public CompareResultsViewModel(
        IResultService resultService)
    {
        _resultService = resultService
            ?? throw new ArgumentNullException(
                nameof(resultService));
    }

    public ObservableCollection<ResultSelectionItem> Results { get; } = [];

    public ObservableCollection<ComparisonMetricViewModel> Metrics { get; } = [];

    public ResultSelectionItem? SelectedFirstResult
    {
        get => _selectedFirstResult;
        set
        {
            if (!SetField(ref _selectedFirstResult, value))
            {
                return;
            }

            BuildComparison();
            OnPropertyChanged(nameof(HasSelectedResults));
        }
    }

    public ResultSelectionItem? SelectedSecondResult
    {
        get => _selectedSecondResult;
        set
        {
            if (!SetField(ref _selectedSecondResult, value))
            {
                return;
            }

            BuildComparison();
            OnPropertyChanged(nameof(HasSelectedResults));
        }
    }

    public bool IsLoading
    {
        get => _isLoading;
        private set => SetField(ref _isLoading, value);
    }

    public string? ErrorMessage
    {
        get => _errorMessage;
        private set
        {
            if (SetField(ref _errorMessage, value))
            {
                OnPropertyChanged(nameof(HasError));
            }
        }
    }

    public bool HasError =>
        !string.IsNullOrWhiteSpace(ErrorMessage);

    public bool HasSelectedResults =>
        SelectedFirstResult is not null
        && SelectedSecondResult is not null
        && SelectedFirstResult.Id != SelectedSecondResult.Id;

    public event PropertyChangedEventHandler? PropertyChanged;

    public async Task LoadAsync()
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            Results.Clear();

            IEnumerable<BenchmarkResult> results =
                await _resultService.GetAllResultsAsync();

            foreach (BenchmarkResult result in results
                         .OrderByDescending(x => x.CompletedAtUtc))
            {
                Results.Add(new ResultSelectionItem
                {
                    Result = result
                });
            }

            if (Results.Count >= 2)
            {
                SelectedFirstResult = Results[0];
                SelectedSecondResult = Results[1];
            }
            else if (Results.Count == 1)
            {
                SelectedFirstResult = Results[0];
                ErrorMessage =
    "At least two completed benchmark results are required. " +
    "Run another benchmark before opening comparison.";
            }
            else
            {
                ErrorMessage =
    "No completed benchmark results were found. " +
    "Run a benchmark first.";
            }
        }
        catch (Exception exception)
{
    ErrorMessage =
        ErrorMessageProvider.GetMessage(
            "Could not load benchmark results",
            exception);
}
        finally
        {
            IsLoading = false;
        }
    }

    private void BuildComparison()
{
    Metrics.Clear();
    ErrorMessage = null;

    ClearComparisonChart();

    if (SelectedFirstResult is null
        || SelectedSecondResult is null)
    {
        OnPropertyChanged(nameof(HasSelectedResults));
        return;
    }

    if (SelectedFirstResult.Id ==
        SelectedSecondResult.Id)
    {
        ErrorMessage =
    "The same result cannot be compared with itself. " +
    "Select two different benchmark runs.";

        OnPropertyChanged(nameof(HasSelectedResults));
        return;
    }

    BenchmarkResult first =
        SelectedFirstResult.Result;

    BenchmarkResult second =
        SelectedSecondResult.Result;

    AddHigherIsBetter(
        "Score",
        (double)first.Statistics.Score.Value,
        (double)second.Statistics.Score.Value,
        value => value.ToString("0.##"));

    AddLowerIsBetter(
        "Average month time",
        first.Statistics.AverageMonthTime.TotalSeconds,
        second.Statistics.AverageMonthTime.TotalSeconds,
        FormatSeconds);

    AddLowerIsBetter(
        "Median month time",
        first.Statistics.MedianMonthTime.TotalSeconds,
        second.Statistics.MedianMonthTime.TotalSeconds,
        FormatSeconds);

    AddLowerIsBetter(
        "Fastest month",
        first.Statistics.MinimumMonthTime.TotalSeconds,
        second.Statistics.MinimumMonthTime.TotalSeconds,
        FormatSeconds);

    AddLowerIsBetter(
        "Slowest month",
        first.Statistics.MaximumMonthTime.TotalSeconds,
        second.Statistics.MaximumMonthTime.TotalSeconds,
        FormatSeconds);

    AddLowerIsBetter(
        "Standard deviation",
        first.Statistics.StandardDeviation.TotalSeconds,
        second.Statistics.StandardDeviation.TotalSeconds,
        FormatSeconds);

    AddHigherIsBetter(
        "Measured months",
        first.Statistics.MeasuredMonthCount,
        second.Statistics.MeasuredMonthCount,
        value => value.ToString("0"));

    BuildComparisonChart(first, second);

    OnPropertyChanged(nameof(HasSelectedResults));
}

    private void AddHigherIsBetter(
        string name,
        double first,
        double second,
        Func<double, string> formatter)
    {
        Metrics.Add(CreateMetric(
            name,
            first,
            second,
            formatter,
            true));
    }

    private void AddLowerIsBetter(
        string name,
        double first,
        double second,
        Func<double, string> formatter)
    {
        Metrics.Add(CreateMetric(
            name,
            first,
            second,
            formatter,
            false));
    }

    private static ComparisonMetricViewModel CreateMetric(
        string name,
        double first,
        double second,
        Func<double, string> formatter,
        bool higherIsBetter)
    {
        bool equal = Math.Abs(first - second) < 0.000001;

        bool firstBetter =
            !equal &&
            (higherIsBetter ? first > second : first < second);

        bool secondBetter =
            !equal &&
            (higherIsBetter ? second > first : second < first);

        double? difference =
            Math.Abs(first) < 0.000001
                ? null
                : ((second - first) / Math.Abs(first)) * 100;

        return new ComparisonMetricViewModel
        {
            Name = name,
            FirstValue = formatter(first),
            SecondValue = formatter(second),
            Difference = difference is null
                ? "N/A"
                : $"{difference.Value:+0.0;-0.0;0.0}%",
            IsFirstBetter = firstBetter,
            IsSecondBetter = secondBetter
        };
    }

    private static string FormatSeconds(double value)
    {
        return $"{value:0.###} s";
    }

    private bool SetField<T>(
        ref T field,
        T value,
        [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(propertyName);

        return true;
    }

    private void OnPropertyChanged(
        [CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(
            this,
            new PropertyChangedEventArgs(propertyName));
    }

    private void BuildComparisonChart(
    BenchmarkResult first,
    BenchmarkResult second)
{
    MonthlyMeasurement[] firstMeasurements =
        first.Measurements
            .Where(measurement => !measurement.IsWarmup)
            .OrderBy(measurement => measurement.Index)
            .ToArray();

    MonthlyMeasurement[] secondMeasurements =
        second.Measurements
            .Where(measurement => !measurement.IsWarmup)
            .OrderBy(measurement => measurement.Index)
            .ToArray();

    int sharedMeasurementCount = Math.Min(
        firstMeasurements.Length,
        secondMeasurements.Length);

    if (sharedMeasurementCount == 0)
    {
        ClearComparisonChart();
        return;
    }

    double[] firstValues = firstMeasurements
        .Take(sharedMeasurementCount)
        .Select(measurement =>
            measurement.ElapsedTime.TotalSeconds)
        .ToArray();

    double[] secondValues = secondMeasurements
        .Take(sharedMeasurementCount)
        .Select(measurement =>
            measurement.ElapsedTime.TotalSeconds)
        .ToArray();

    string[] labels = firstMeasurements
        .Take(sharedMeasurementCount)
        .Select(measurement =>
            measurement.ToDate.ToString())
        .ToArray();

    var primaryTextPaint = new SolidColorPaint(
        new SKColor(249, 250, 251));

    var secondaryTextPaint = new SolidColorPaint(
        new SKColor(156, 163, 175));

    var separatorPaint = new SolidColorPaint(
        new SKColor(55, 65, 81))
    {
        StrokeThickness = 1
    };

    ComparisonSeries =
    [
        new LineSeries<double>
        {
            Name = CreateSeriesName(
                first,
                "First result"),

            Values = firstValues,
            Fill = null,
            GeometrySize = 7,
            LineSmoothness = 0,

            Stroke = new SolidColorPaint(
                new SKColor(59, 130, 246))
            {
                StrokeThickness = 3
            },

            GeometryFill = new SolidColorPaint(
                new SKColor(59, 130, 246)),

            GeometryStroke = new SolidColorPaint(
                new SKColor(37, 99, 235))
            {
                StrokeThickness = 2
            },

            YToolTipLabelFormatter = point =>
                $"{point.Coordinate.PrimaryValue:0.###} s"
        },

        new LineSeries<double>
        {
            Name = CreateSeriesName(
                second,
                "Second result"),

            Values = secondValues,
            Fill = null,
            GeometrySize = 7,
            LineSmoothness = 0,

            Stroke = new SolidColorPaint(
                new SKColor(34, 197, 94))
            {
                StrokeThickness = 3
            },

            GeometryFill = new SolidColorPaint(
                new SKColor(34, 197, 94)),

            GeometryStroke = new SolidColorPaint(
                new SKColor(22, 163, 74))
            {
                StrokeThickness = 2
            },

            YToolTipLabelFormatter = point =>
                $"{point.Coordinate.PrimaryValue:0.###} s"
        }
    ];

    ComparisonXAxes =
    [
        new Axis
        {
            Name = "Game month",
            Labels = labels,
            LabelsRotation = 30,
            TextSize = 12,
            LabelsPaint = secondaryTextPaint,
            NamePaint = primaryTextPaint,
            SeparatorsPaint = separatorPaint,
            MinStep = 1,
            ForceStepToMin = true
        }
    ];

    ComparisonYAxes =
    [
        new Axis
        {
            Name = "Duration, seconds",
            TextSize = 12,
            LabelsPaint = secondaryTextPaint,
            NamePaint = primaryTextPaint,
            SeparatorsPaint = separatorPaint,
            Labeler = value =>
                $"{value:0.##} s",
            MinLimit = 0
        }
    ];

    OnPropertyChanged(nameof(HasChartData));
}

private void ClearComparisonChart()
{
    ComparisonSeries = [];
    ComparisonXAxes = [];
    ComparisonYAxes = [];

    OnPropertyChanged(nameof(HasChartData));
}

private static string CreateSeriesName(
    BenchmarkResult result,
    string fallbackName)
{
    return string.IsNullOrWhiteSpace(result.Name)
        ? fallbackName
        : result.Name;
}

}