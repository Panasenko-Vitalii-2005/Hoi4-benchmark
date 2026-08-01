using HOI4Benchmark.Domain.Common;
using HOI4Benchmark.Domain.Statistics;

namespace HOI4Benchmark.Domain.Benchmarks;

public sealed class BenchmarkSession
{
    private readonly List<MonthlyMeasurement> _measurements = [];

    public BenchmarkSession(
        Guid id,
        string name,
        DateTimeOffset startedAtUtc,
        int targetMeasuredMonths,
        int warmupMonths = 0)
    {
        if (id == Guid.Empty)
        {
            throw new DomainException("Benchmark session id cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Benchmark session name is required.");
        }

        if (targetMeasuredMonths < 1)
        {
            throw new DomainException("Benchmark session target measured months must be greater than zero.");
        }

        if (warmupMonths < 0)
        {
            throw new DomainException("Benchmark session warm-up months cannot be negative.");
        }

        Id = id;
        Name = name.Trim();
        StartedAtUtc = startedAtUtc;
        TargetMeasuredMonths = targetMeasuredMonths;
        WarmupMonths = warmupMonths;
        Status = BenchmarkStatus.NotStarted;
    }

    public Guid Id { get; }

    public string Name { get; }

    public DateTimeOffset StartedAtUtc { get; }

    public DateTimeOffset? CompletedAtUtc { get; private set; }

    public int TargetMeasuredMonths { get; }

    public int WarmupMonths { get; }

    public BenchmarkStatus Status { get; private set; }

    public IReadOnlyList<MonthlyMeasurement> Measurements => _measurements;

    public int MeasuredMonthCount => _measurements.Count(measurement => !measurement.IsWarmup);

    public void MarkWaitingForAutosave()
    {
        EnsureNotFinished();
        Status = BenchmarkStatus.WaitingForAutosave;
    }

    public void AddMeasurement(MonthlyMeasurement measurement)
    {
        ArgumentNullException.ThrowIfNull(measurement);
        EnsureNotFinished();

        if (_measurements.Any(existing => existing.Index == measurement.Index))
        {
            throw new DomainException($"Benchmark session already contains measurement with index {measurement.Index}.");
        }

        _measurements.Add(measurement);
        Status = BenchmarkStatus.Running;
    }

    public BenchmarkResult Complete(DateTimeOffset completedAtUtc, StatisticsCalculator? statisticsCalculator = null)
    {
        EnsureNotFinished();

        if (completedAtUtc < StartedAtUtc)
        {
            throw new DomainException("Benchmark session completion timestamp cannot be earlier than start timestamp.");
        }

        if (MeasuredMonthCount < TargetMeasuredMonths)
        {
            throw new DomainException("Benchmark session cannot be completed before target measured month count is reached.");
        }

        statisticsCalculator ??= new StatisticsCalculator();
        BenchmarkStatistics statistics = statisticsCalculator.Calculate(_measurements);

        CompletedAtUtc = completedAtUtc;
        Status = BenchmarkStatus.Completed;

        return new BenchmarkResult(
            Id,
            Name,
            StartedAtUtc,
            completedAtUtc,
            _measurements.OrderBy(measurement => measurement.Index).ToArray(),
            statistics);
    }

    public void Cancel(DateTimeOffset completedAtUtc)
    {
        EnsureNotFinished();
        CompletedAtUtc = completedAtUtc;
        Status = BenchmarkStatus.Cancelled;
    }

    public void Fail(DateTimeOffset completedAtUtc)
    {
        EnsureNotFinished();
        CompletedAtUtc = completedAtUtc;
        Status = BenchmarkStatus.Failed;
    }

    private void EnsureNotFinished()
    {
        if (Status is BenchmarkStatus.Completed or BenchmarkStatus.Cancelled or BenchmarkStatus.Failed)
        {
            throw new DomainException("Benchmark session is already finished.");
        }
    }
}
