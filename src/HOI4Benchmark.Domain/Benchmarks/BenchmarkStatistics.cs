using HOI4Benchmark.Domain.Common;

namespace HOI4Benchmark.Domain.Benchmarks;

public sealed record BenchmarkStatistics
{
    public BenchmarkStatistics(
        int measuredMonthCount,
        TimeSpan averageMonthTime,
        TimeSpan medianMonthTime,
        TimeSpan minimumMonthTime,
        TimeSpan maximumMonthTime,
        TimeSpan standardDeviation,
        TimeSpan totalMeasuredTime,
        TimeSpan estimatedYearTime,
        BenchmarkScore score)
    {
        if (measuredMonthCount < 1)
        {
            throw new DomainException("Benchmark statistics require at least one measured month.");
        }

        if (averageMonthTime <= TimeSpan.Zero)
        {
            throw new DomainException("Average month time must be greater than zero.");
        }

        if (medianMonthTime <= TimeSpan.Zero)
        {
            throw new DomainException("Median month time must be greater than zero.");
        }

        if (minimumMonthTime <= TimeSpan.Zero)
        {
            throw new DomainException("Minimum month time must be greater than zero.");
        }

        if (maximumMonthTime <= TimeSpan.Zero)
        {
            throw new DomainException("Maximum month time must be greater than zero.");
        }

        if (standardDeviation < TimeSpan.Zero)
        {
            throw new DomainException("Standard deviation cannot be negative.");
        }

        if (totalMeasuredTime <= TimeSpan.Zero)
        {
            throw new DomainException("Total measured time must be greater than zero.");
        }

        if (estimatedYearTime <= TimeSpan.Zero)
        {
            throw new DomainException("Estimated year time must be greater than zero.");
        }

        MeasuredMonthCount = measuredMonthCount;
        AverageMonthTime = averageMonthTime;
        MedianMonthTime = medianMonthTime;
        MinimumMonthTime = minimumMonthTime;
        MaximumMonthTime = maximumMonthTime;
        StandardDeviation = standardDeviation;
        TotalMeasuredTime = totalMeasuredTime;
        EstimatedYearTime = estimatedYearTime;
        Score = score;
    }

    public int MeasuredMonthCount { get; }

    public TimeSpan AverageMonthTime { get; }

    public TimeSpan MedianMonthTime { get; }

    public TimeSpan MinimumMonthTime { get; }

    public TimeSpan MaximumMonthTime { get; }

    public TimeSpan StandardDeviation { get; }

    public TimeSpan TotalMeasuredTime { get; }

    public TimeSpan EstimatedYearTime { get; }

    public BenchmarkScore Score { get; }
}
