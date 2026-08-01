using System.Text.Json;
using HOI4Benchmark.Domain.Benchmarks;
using HOI4Benchmark.Domain.Game;
using HOI4Benchmark.Infrastructure.Export;
namespace HOI4Benchmark.Tests.Infrastructure;

public sealed class ExportServiceTests
{
    [Fact]
    public async Task ExportResultToJsonAsync_ShouldSerializeResult()
    {
        var service = new ExportService();
        var result = CreateResult();

        string json =
            await service.ExportResultToJsonAsync(result);

        using JsonDocument document =
            JsonDocument.Parse(json);

        JsonElement root = document.RootElement;

        Assert.Equal(
            result.Id,
            root.GetProperty("id").GetGuid());

        Assert.Equal(
            result.Name,
            root.GetProperty("name").GetString());

        Assert.Equal(
            result.SchemaVersion,
            root.GetProperty("schemaVersion").GetString());

        Assert.Equal(
            1,
            root.GetProperty("measurements")
                .GetArrayLength());
    }

    [Fact]
    public async Task ExportToJsonAsync_ShouldSerializeAllResults()
    {
        var service = new ExportService();

        var first = CreateResult(
            name: "First benchmark");

        var second = CreateResult(
            name: "Second benchmark");

        string json = await service.ExportToJsonAsync(
            [first, second]);

        using JsonDocument document =
            JsonDocument.Parse(json);

        JsonElement root = document.RootElement;

        Assert.Equal(
            JsonValueKind.Array,
            root.ValueKind);

        Assert.Equal(
            2,
            root.GetArrayLength());

        Assert.Equal(
            "First benchmark",
            root[0].GetProperty("name").GetString());

        Assert.Equal(
            "Second benchmark",
            root[1].GetProperty("name").GetString());
    }

    [Fact]
    public async Task ExportToJsonAsync_WhenCollectionIsEmpty_ReturnsEmptyArray()
    {
        var service = new ExportService();

        string json = await service.ExportToJsonAsync(
            Array.Empty<BenchmarkResult>());

        using JsonDocument document =
            JsonDocument.Parse(json);

        Assert.Equal(
            JsonValueKind.Array,
            document.RootElement.ValueKind);

        Assert.Equal(
            0,
            document.RootElement.GetArrayLength());
    }

    [Fact]
    public async Task ExportResultToJsonAsync_WhenResultIsNull_ThrowsArgumentNullException()
    {
        var service = new ExportService();

        await Assert.ThrowsAsync<ArgumentNullException>(
            () => service.ExportResultToJsonAsync(null!));
    }

    [Fact]
    public async Task ExportToJsonAsync_WhenResultsAreNull_ThrowsArgumentNullException()
    {
        var service = new ExportService();

        await Assert.ThrowsAsync<ArgumentNullException>(
            () => service.ExportToJsonAsync(null!));
    }

    private static BenchmarkResult CreateResult(
        Guid? id = null,
        string name = "Test benchmark")
    {
        var startedAtUtc = new DateTimeOffset(
            2026,
            1,
            1,
            12,
            0,
            0,
            TimeSpan.Zero);

        TimeSpan monthTime =
            TimeSpan.FromSeconds(10);

        var measurement = new MonthlyMeasurement(
            index: 1,
            fromDate: new GameDate(1936, 1, 1),
            toDate: new GameDate(1936, 2, 1),
            elapsedTime: monthTime,
            startedAtUtc: startedAtUtc,
            completedAtUtc:
                startedAtUtc.Add(monthTime));

        var score = new BenchmarkScore(
            value: 1000m,
            formulaVersion: "1.0",
            baselineMonthTimeSeconds: 10m,
            description: "Test score");

        var statistics = new BenchmarkStatistics(
            measuredMonthCount: 1,
            averageMonthTime: monthTime,
            medianMonthTime: monthTime,
            minimumMonthTime: monthTime,
            maximumMonthTime: monthTime,
            standardDeviation: TimeSpan.Zero,
            totalMeasuredTime: monthTime,
            estimatedYearTime:
                TimeSpan.FromSeconds(120),
            score: score);

        return new BenchmarkResult(
            id: id ?? Guid.NewGuid(),
            name: name,
            startedAtUtc: startedAtUtc,
            completedAtUtc:
                startedAtUtc.Add(monthTime),
            measurements: [measurement],
            statistics: statistics);
    }
}