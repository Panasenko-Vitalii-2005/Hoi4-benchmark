using HOI4Benchmark.Domain.Benchmarks;

namespace HOI4Benchmark.App.ViewModels;

public sealed class BenchmarkResultItem
{
    public BenchmarkResultItem(
        BenchmarkResult result)
    {
        Source = result
            ?? throw new ArgumentNullException(nameof(result));
    }

    public BenchmarkResult Source { get; }

    public Guid Id => Source.Id;

    public string Name => Source.Name;

    public string CreatedAt =>
        Source.CompletedAtUtc
            .ToLocalTime()
            .ToString("yyyy-MM-dd HH:mm");

    public string GamePeriod =>
        Source.Measurements.Count == 0
            ? "—"
            : $"{Source.Measurements[0].FromDate} — " +
              $"{Source.Measurements[^1].ToDate}";

    public string Duration =>
        Source.Duration.ToString(@"hh\:mm\:ss");

    public int MeasurementCount =>
        Source.Statistics.MeasuredMonthCount;

    public string AverageMonthTime =>
        FormatSeconds(Source.Statistics.AverageMonthTime);

    public string MedianMonthTime =>
        FormatSeconds(Source.Statistics.MedianMonthTime);

    public string FastestMonthTime =>
        FormatSeconds(Source.Statistics.MinimumMonthTime);

    public string SlowestMonthTime =>
        FormatSeconds(Source.Statistics.MaximumMonthTime);

    public string StandardDeviation =>
        FormatSeconds(Source.Statistics.StandardDeviation);

    public string Score =>
        Source.Statistics.Score.Value.ToString("0.##");

    private static string FormatSeconds(
        TimeSpan value)
    {
        return $"{value.TotalSeconds:0.##} s";
    }
}