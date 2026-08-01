using HOI4Benchmark.Domain.Common;
using HOI4Benchmark.Domain.Game;

namespace HOI4Benchmark.Domain.Benchmarks;

public sealed class MonthlyMeasurement
{
    private readonly IReadOnlyList<string> _warnings;

    public MonthlyMeasurement(
        int index,
        GameDate fromDate,
        GameDate toDate,
        TimeSpan elapsedTime,
        DateTimeOffset startedAtUtc,
        DateTimeOffset completedAtUtc,
        bool isWarmup = false,
        IReadOnlyList<string>? warnings = null)
    {
        if (index < 1)
        {
            throw new DomainException("Monthly measurement index must be greater than zero.");
        }

        if (toDate <= fromDate)
        {
            throw new DomainException("Monthly measurement target date must be after source date.");
        }

        if (elapsedTime <= TimeSpan.Zero)
        {
            throw new DomainException("Monthly measurement elapsed time must be greater than zero.");
        }

        if (completedAtUtc < startedAtUtc)
        {
            throw new DomainException("Monthly measurement completion timestamp cannot be earlier than start timestamp.");
        }

        Index = index;
        FromDate = fromDate;
        ToDate = toDate;
        ElapsedTime = elapsedTime;
        StartedAtUtc = startedAtUtc;
        CompletedAtUtc = completedAtUtc;
        IsWarmup = isWarmup;
        _warnings = warnings?.ToArray() ?? [];
    }

    public int Index { get; }

    public GameDate FromDate { get; }

    public GameDate ToDate { get; }

    public TimeSpan ElapsedTime { get; }

    public DateTimeOffset StartedAtUtc { get; }

    public DateTimeOffset CompletedAtUtc { get; }

    public bool IsWarmup { get; }

    public IReadOnlyList<string> Warnings => _warnings;

    public bool IsExpectedMonthlyTransition => ToDate.IsNextMonthAfter(FromDate);
}
