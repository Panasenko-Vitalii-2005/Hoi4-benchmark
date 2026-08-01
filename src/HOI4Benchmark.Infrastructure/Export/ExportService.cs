using System.Globalization;
using System.Text;
using System.Text.Json;
using HOI4Benchmark.Application.Abstractions;
using HOI4Benchmark.Domain.Benchmarks;
using HOI4Benchmark.Application.Export;

namespace HOI4Benchmark.Infrastructure.Export;

public sealed class ExportService : IExportService
{

    private const string MeasurementCsvHeader =
    "ResultId,ResultName,ResultStartedAtUtc," +
    "ResultCompletedAtUtc,Score," +
    "MeasurementIndex,FromDate,ToDate," +
    "ElapsedSeconds,MeasurementStartedAtUtc," +
    "MeasurementCompletedAtUtc,IsWarmup," +
    "IsExpectedMonthlyTransition,Warnings";

    private static readonly JsonSerializerOptions SerializerOptions =
        new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

    private const string CsvHeader =
        "Id,Name,StartedAtUtc,CompletedAtUtc,DurationSeconds," +
        "MeasuredMonthCount,AverageMonthTimeSeconds,MedianMonthTimeSeconds," +
        "MinimumMonthTimeSeconds,MaximumMonthTimeSeconds," +
        "StandardDeviationSeconds,TotalMeasuredTimeSeconds," +
        "EstimatedYearTimeSeconds,Score,ScoreFormulaVersion," +
        "BaselineMonthTimeSeconds,SchemaVersion";

    public Task<string> ExportToJsonAsync(
        IEnumerable<BenchmarkResult> results)
    {
        ArgumentNullException.ThrowIfNull(results);

        string json = JsonSerializer.Serialize(
            results.ToList(),
            SerializerOptions);

        return Task.FromResult(json);
    }

    public Task<string> ExportResultToJsonAsync(
        BenchmarkResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        string json = JsonSerializer.Serialize(
            result,
            SerializerOptions);

        return Task.FromResult(json);
    }

    public Task<string> ExportToCsvAsync(
        IEnumerable<BenchmarkResult> results)
    {
        ArgumentNullException.ThrowIfNull(results);

        var builder = new StringBuilder();

        builder.AppendLine(CsvHeader);

        foreach (BenchmarkResult result in results)
        {
            builder.AppendLine(CreateCsvRow(result));
        }

        return Task.FromResult(builder.ToString());
    }

    public Task<string> ExportResultToCsvAsync(
        BenchmarkResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        var builder = new StringBuilder();

        builder.AppendLine(CsvHeader);
        builder.AppendLine(CreateCsvRow(result));

        return Task.FromResult(builder.ToString());
    }

    private static string CreateCsvRow(
        BenchmarkResult result)
    {
        BenchmarkStatistics statistics =
            result.Statistics;

        BenchmarkScore score =
            statistics.Score;

        TimeSpan duration =
            result.CompletedAtUtc - result.StartedAtUtc;

        string[] values =
        [
            result.Id.ToString(),
            EscapeCsv(result.Name),
            FormatDateTime(result.StartedAtUtc),
            FormatDateTime(result.CompletedAtUtc),
            FormatDecimal((decimal)duration.TotalSeconds),

            statistics.MeasuredMonthCount.ToString(
                CultureInfo.InvariantCulture),

            FormatSeconds(statistics.AverageMonthTime),
            FormatSeconds(statistics.MedianMonthTime),
            FormatSeconds(statistics.MinimumMonthTime),
            FormatSeconds(statistics.MaximumMonthTime),
            FormatSeconds(statistics.StandardDeviation),
            FormatSeconds(statistics.TotalMeasuredTime),
            FormatSeconds(statistics.EstimatedYearTime),

            FormatDecimal(score.Value),
            EscapeCsv(score.FormulaVersion),
            FormatDecimal(score.BaselineMonthTimeSeconds),

            EscapeCsv(result.SchemaVersion)
        ];

        return string.Join(",", values);
    }

    private static string FormatDateTime(
        DateTimeOffset value)
    {
        return value.ToUniversalTime().ToString(
            "O",
            CultureInfo.InvariantCulture);
    }

    private static string FormatSeconds(
        TimeSpan value)
    {
        return value.TotalSeconds.ToString(
            "0.########",
            CultureInfo.InvariantCulture);
    }

    private static string FormatDecimal(
        decimal value)
    {
        return value.ToString(
            "0.########",
            CultureInfo.InvariantCulture);
    }

    private static string EscapeCsv(
        string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        bool requiresQuotes =
            value.Contains(',') ||
            value.Contains('"') ||
            value.Contains('\n') ||
            value.Contains('\r');

        if (!requiresQuotes)
        {
            return value;
        }

        string escaped =
            value.Replace("\"", "\"\"");

        return $"\"{escaped}\"";
    }

    public Task<string> ExportDetailedResultToJsonAsync(
    BenchmarkResult result,
    ExportPrivacyOptions privacyOptions)
{
    ArgumentNullException.ThrowIfNull(result);
    ArgumentNullException.ThrowIfNull(privacyOptions);

    var document = new
    {
        exportVersion = "1.0",
        generatedAtUtc = DateTimeOffset.UtcNow,
        application = "HOI4 Benchmark",
        privacy = privacyOptions,
        result = CreateDetailedResult(
            result,
            privacyOptions,
            resultNumber: 1)
    };

    string json = JsonSerializer.Serialize(
        document,
        SerializerOptions);

    return Task.FromResult(json);
}

    public Task<string> ExportDetailedResultsToJsonAsync(
    IEnumerable<BenchmarkResult> results,
    ExportPrivacyOptions privacyOptions)
{
    ArgumentNullException.ThrowIfNull(results);
    ArgumentNullException.ThrowIfNull(privacyOptions);

    BenchmarkResult[] resultArray =
        results.ToArray();

    var document = new
    {
        exportVersion = "1.0",
        generatedAtUtc = DateTimeOffset.UtcNow,
        application = "HOI4 Benchmark",
        privacy = privacyOptions,
        resultCount = resultArray.Length,

        results = resultArray.Select(
            (result, index) => CreateDetailedResult(
                result,
                privacyOptions,
                index + 1))
    };

    string json = JsonSerializer.Serialize(
        document,
        SerializerOptions);

    return Task.FromResult(json);
}

    private static object CreateDetailedResult(
    BenchmarkResult result,
    ExportPrivacyOptions privacyOptions,
    int resultNumber)
{
    string exportedName =
        privacyOptions.AnonymizeResultNames
            ? $"Result {resultNumber}"
            : result.Name;

    DateTimeOffset? startedAtUtc =
        privacyOptions.ExcludeExactTimestamps
            ? null
            : result.StartedAtUtc;

    DateTimeOffset? completedAtUtc =
        privacyOptions.ExcludeExactTimestamps
            ? null
            : result.CompletedAtUtc;

    return new
    {
        id = result.Id,
        name = exportedName,
        schemaVersion = result.SchemaVersion,
        startedAtUtc,
        completedAtUtc,
        durationSeconds = result.Duration.TotalSeconds,

        statistics = new
        {
            measuredMonthCount =
                result.Statistics.MeasuredMonthCount,

            averageMonthTimeSeconds =
                result.Statistics
                    .AverageMonthTime
                    .TotalSeconds,

            medianMonthTimeSeconds =
                result.Statistics
                    .MedianMonthTime
                    .TotalSeconds,

            minimumMonthTimeSeconds =
                result.Statistics
                    .MinimumMonthTime
                    .TotalSeconds,

            maximumMonthTimeSeconds =
                result.Statistics
                    .MaximumMonthTime
                    .TotalSeconds,

            standardDeviationSeconds =
                result.Statistics
                    .StandardDeviation
                    .TotalSeconds,

            totalMeasuredTimeSeconds =
                result.Statistics
                    .TotalMeasuredTime
                    .TotalSeconds,

            estimatedYearTimeSeconds =
                result.Statistics
                    .EstimatedYearTime
                    .TotalSeconds,

            score = new
            {
                value =
                    result.Statistics.Score.Value,

                formulaVersion =
                    result.Statistics.Score.FormulaVersion,

                baselineMonthTimeSeconds =
                    result.Statistics.Score
                        .BaselineMonthTimeSeconds,

                description =
                    result.Statistics.Score.Description
            }
        },

        measurements = result.Measurements
            .OrderBy(measurement => measurement.Index)
            .Select(measurement => new
            {
                index = measurement.Index,

                fromDate =
                    measurement.FromDate.ToString(),

                toDate =
                    measurement.ToDate.ToString(),

                elapsedSeconds =
                    measurement.ElapsedTime.TotalSeconds,

                startedAtUtc =
                    privacyOptions.ExcludeExactTimestamps
                        ? (DateTimeOffset?)null
                        : measurement.StartedAtUtc,

                completedAtUtc =
                    privacyOptions.ExcludeExactTimestamps
                        ? (DateTimeOffset?)null
                        : measurement.CompletedAtUtc,

                isWarmup =
                    measurement.IsWarmup,

                isExpectedMonthlyTransition =
                    measurement.IsExpectedMonthlyTransition,

                warnings =
                    privacyOptions.ExcludeWarnings
                        ? Array.Empty<string>()
                        : measurement.Warnings
            })
    };
}
    public Task<string> ExportMeasurementsToCsvAsync(
    BenchmarkResult result,
    ExportPrivacyOptions privacyOptions)
{
    ArgumentNullException.ThrowIfNull(result);
    ArgumentNullException.ThrowIfNull(privacyOptions);

    return ExportMeasurementsToCsvAsync(
        [result],
        privacyOptions);
}

    public Task<string> ExportMeasurementsToCsvAsync(
    IEnumerable<BenchmarkResult> results,
    ExportPrivacyOptions privacyOptions)
{
    ArgumentNullException.ThrowIfNull(results);
    ArgumentNullException.ThrowIfNull(privacyOptions);

    var builder = new StringBuilder();

    builder.AppendLine(MeasurementCsvHeader);

    int resultNumber = 0;

    foreach (BenchmarkResult result in results)
    {
        resultNumber++;

        foreach (MonthlyMeasurement measurement
                 in result.Measurements
                     .OrderBy(item => item.Index))
        {
            builder.AppendLine(
                CreateMeasurementCsvRow(
                    result,
                    measurement,
                    privacyOptions,
                    resultNumber));
        }
    }

    return Task.FromResult(builder.ToString());
}

private static string CreateMeasurementCsvRow(
    BenchmarkResult result,
    MonthlyMeasurement measurement,
    ExportPrivacyOptions privacyOptions,
    int resultNumber)
{
    string exportedName =
        privacyOptions.AnonymizeResultNames
            ? $"Result {resultNumber}"
            : result.Name;

    string resultStartedAt =
        privacyOptions.ExcludeExactTimestamps
            ? string.Empty
            : FormatDateTime(result.StartedAtUtc);

    string resultCompletedAt =
        privacyOptions.ExcludeExactTimestamps
            ? string.Empty
            : FormatDateTime(result.CompletedAtUtc);

    string measurementStartedAt =
        privacyOptions.ExcludeExactTimestamps
            ? string.Empty
            : FormatDateTime(measurement.StartedAtUtc);

    string measurementCompletedAt =
        privacyOptions.ExcludeExactTimestamps
            ? string.Empty
            : FormatDateTime(measurement.CompletedAtUtc);

    string warnings =
        privacyOptions.ExcludeWarnings
            ? string.Empty
            : string.Join(
                " | ",
                measurement.Warnings);

    string[] values =
    [
        result.Id.ToString(),

        EscapeCsv(exportedName),

        resultStartedAt,

        resultCompletedAt,

        FormatDecimal(
            result.Statistics.Score.Value),

        measurement.Index.ToString(
            CultureInfo.InvariantCulture),

        measurement.FromDate.ToString(),

        measurement.ToDate.ToString(),

        FormatSeconds(
            measurement.ElapsedTime),

        measurementStartedAt,

        measurementCompletedAt,

        measurement.IsWarmup
            ? "true"
            : "false",

        measurement.IsExpectedMonthlyTransition
            ? "true"
            : "false",

        EscapeCsv(warnings)
    ];

    return string.Join(",", values);
}

}