using HOI4Benchmark.Domain.Benchmarks;
using HOI4Benchmark.Domain.Common;

namespace HOI4Benchmark.Domain.Statistics;

public sealed class StatisticsCalculator
{
    private readonly BenchmarkScoreCalculator _scoreCalculator;

    public StatisticsCalculator()
        : this(new BenchmarkScoreCalculator())
    {
    }

    public StatisticsCalculator(BenchmarkScoreCalculator scoreCalculator)
    {
        _scoreCalculator = scoreCalculator;
    }

    public BenchmarkStatistics Calculate(IEnumerable<MonthlyMeasurement> measurements)
    {
        ArgumentNullException.ThrowIfNull(measurements);

        MonthlyMeasurement[] measuredMonths = measurements
            .Where(measurement => !measurement.IsWarmup)
            .OrderBy(measurement => measurement.Index)
            .ToArray();

        if (measuredMonths.Length == 0)
        {
            throw new DomainException("Cannot calculate benchmark statistics without measured months.");
        }

        TimeSpan[] elapsedTimes = measuredMonths
            .Select(measurement => measurement.ElapsedTime)
            .OrderBy(time => time.Ticks)
            .ToArray();

        TimeSpan average = Average(elapsedTimes);
        TimeSpan median = Median(elapsedTimes);
        TimeSpan minimum = elapsedTimes[0];
        TimeSpan maximum = elapsedTimes[^1];
        TimeSpan standardDeviation = StandardDeviation(elapsedTimes, average);
        TimeSpan total = TimeSpan.FromTicks(elapsedTimes.Sum(time => time.Ticks));
        TimeSpan estimatedYear = TimeSpan.FromTicks(average.Ticks * 12);
        BenchmarkScore score = _scoreCalculator.Calculate(average);

        return new BenchmarkStatistics(
            measuredMonths.Length,
            average,
            median,
            minimum,
            maximum,
            standardDeviation,
            total,
            estimatedYear,
            score);
    }

    private static TimeSpan Average(IReadOnlyCollection<TimeSpan> values)
    {
        long averageTicks = Convert.ToInt64(values.Average(value => value.Ticks));
        return TimeSpan.FromTicks(averageTicks);
    }

    private static TimeSpan Median(IReadOnlyList<TimeSpan> sortedValues)
    {
        int middle = sortedValues.Count / 2;

        if (sortedValues.Count % 2 != 0)
        {
            return sortedValues[middle];
        }

        long medianTicks = (sortedValues[middle - 1].Ticks + sortedValues[middle].Ticks) / 2;
        return TimeSpan.FromTicks(medianTicks);
    }

    private static TimeSpan StandardDeviation(IReadOnlyCollection<TimeSpan> values, TimeSpan average)
    {
        double averageSeconds = average.TotalSeconds;
        double variance = values.Average(value => Math.Pow(value.TotalSeconds - averageSeconds, 2));
        return TimeSpan.FromSeconds(Math.Sqrt(variance));
    }
}
