using HOI4Benchmark.Domain.Benchmarks;
using HOI4Benchmark.Domain.Common;
using HOI4Benchmark.Domain.Game;

namespace HOI4Benchmark.Tests.Domain;

public sealed class BenchmarkSessionTests
{
    [Fact]
    public void AddMeasurement_WhenMeasurementIsValid_AddsMeasurementAndMarksSessionAsRunning()
    {
        BenchmarkSession session = CreateSession(targetMeasuredMonths: 1);
        MonthlyMeasurement measurement = CreateMeasurement(1, 10);

        session.AddMeasurement(measurement);

        Assert.Equal(BenchmarkStatus.Running, session.Status);
        Assert.Single(session.Measurements);
        Assert.Equal(1, session.MeasuredMonthCount);
    }

    [Fact]
    public void Complete_WhenTargetMeasuredMonthCountIsReached_ReturnsBenchmarkResult()
    {
        DateTimeOffset startedAtUtc = new(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        BenchmarkSession session = new(Guid.NewGuid(), "Test benchmark", startedAtUtc, targetMeasuredMonths: 2, warmupMonths: 1);

        session.AddMeasurement(CreateMeasurement(1, 100, isWarmup: true));
        session.AddMeasurement(CreateMeasurement(2, 10));
        session.AddMeasurement(CreateMeasurement(3, 20));

        BenchmarkResult result = session.Complete(startedAtUtc.AddMinutes(1));

        Assert.Equal(BenchmarkStatus.Completed, session.Status);
        Assert.Equal(session.Id, result.Id);
        Assert.Equal("Test benchmark", result.Name);
        Assert.Equal(3, result.Measurements.Count);
        Assert.Equal(2, result.Statistics.MeasuredMonthCount);
        Assert.Equal(TimeSpan.FromSeconds(15), result.Statistics.AverageMonthTime);
    }

    [Fact]
    public void Complete_WhenTargetMeasuredMonthCountIsNotReached_ThrowsDomainException()
    {
        BenchmarkSession session = CreateSession(targetMeasuredMonths: 2);

        session.AddMeasurement(CreateMeasurement(1, 10));

        Assert.Throws<DomainException>(() => session.Complete(DateTimeOffset.UtcNow));
    }

    [Fact]
    public void AddMeasurement_WhenSessionIsCompleted_ThrowsDomainException()
    {
        BenchmarkSession session = CreateSession(targetMeasuredMonths: 1);
        session.AddMeasurement(CreateMeasurement(1, 10));
        session.Complete(DateTimeOffset.UtcNow.AddMinutes(1));

        Assert.Throws<DomainException>(() => session.AddMeasurement(CreateMeasurement(2, 20)));
    }

    private static BenchmarkSession CreateSession(int targetMeasuredMonths)
    {
        return new BenchmarkSession(
            Guid.NewGuid(),
            "Test benchmark",
            new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero),
            targetMeasuredMonths);
    }

    private static MonthlyMeasurement CreateMeasurement(int index, double elapsedSeconds, bool isWarmup = false)
    {
        DateTimeOffset startedAtUtc = new(2025, 1, 1, 0, 0, index, TimeSpan.Zero);
        DateTimeOffset completedAtUtc = startedAtUtc.AddSeconds(elapsedSeconds);
        GameDate fromDate = new(1936, index, 1);
        GameDate toDate = fromDate.AddMonths(1);

        return new MonthlyMeasurement(
            index,
            fromDate,
            toDate,
            TimeSpan.FromSeconds(elapsedSeconds),
            startedAtUtc,
            completedAtUtc,
            isWarmup);
    }
}
