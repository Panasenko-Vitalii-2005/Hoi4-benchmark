using HOI4Benchmark.Domain.Benchmarks;
using HOI4Benchmark.Domain.Common;
using HOI4Benchmark.Domain.Game;
using HOI4Benchmark.Domain.Statistics;

namespace HOI4Benchmark.Tests.Domain;

public sealed class StatisticsCalculatorTests
{
    [Fact]
    public void Calculate_ExcludesWarmupMeasurementsAndCalculatesStatistics()
    {
        MonthlyMeasurement[] measurements =
        [
            CreateMeasurement(1, 100, isWarmup: true),
            CreateMeasurement(2, 10),
            CreateMeasurement(3, 12),
            CreateMeasurement(4, 14),
        ];
        StatisticsCalculator calculator = new();

        BenchmarkStatistics statistics = calculator.Calculate(measurements);

        Assert.Equal(3, statistics.MeasuredMonthCount);
        Assert.Equal(TimeSpan.FromSeconds(12), statistics.AverageMonthTime);
        Assert.Equal(TimeSpan.FromSeconds(12), statistics.MedianMonthTime);
        Assert.Equal(TimeSpan.FromSeconds(10), statistics.MinimumMonthTime);
        Assert.Equal(TimeSpan.FromSeconds(14), statistics.MaximumMonthTime);
        Assert.Equal(TimeSpan.FromSeconds(36), statistics.TotalMeasuredTime);
        Assert.Equal(TimeSpan.FromSeconds(144), statistics.EstimatedYearTime);
        Assert.Equal(833.33m, statistics.Score.Value);
        Assert.Equal(BenchmarkScoreCalculator.FormulaVersion, statistics.Score.FormulaVersion);
        Assert.Equal(1.633, statistics.StandardDeviation.TotalSeconds, precision: 3);
    }

    [Fact]
    public void Calculate_WhenOnlyWarmupMeasurementsExist_ThrowsDomainException()
    {
        MonthlyMeasurement[] measurements = [CreateMeasurement(1, 100, isWarmup: true)];
        StatisticsCalculator calculator = new();

        Assert.Throws<DomainException>(() => calculator.Calculate(measurements));
    }

    [Fact]
    public void Calculate_WhenMeasurementCountIsEven_CalculatesMedianAsAverageOfMiddleValues()
    {
        MonthlyMeasurement[] measurements =
        [
            CreateMeasurement(1, 10),
            CreateMeasurement(2, 20),
            CreateMeasurement(3, 30),
            CreateMeasurement(4, 40),
        ];
        StatisticsCalculator calculator = new();

        BenchmarkStatistics statistics = calculator.Calculate(measurements);

        Assert.Equal(TimeSpan.FromSeconds(25), statistics.MedianMonthTime);
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
