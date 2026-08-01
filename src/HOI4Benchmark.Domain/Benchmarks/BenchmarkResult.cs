using HOI4Benchmark.Domain.Common;

namespace HOI4Benchmark.Domain.Benchmarks;

public sealed class BenchmarkResult
{
    private readonly IReadOnlyList<MonthlyMeasurement> _measurements;

    public BenchmarkResult(
        Guid id,
        string name,
        DateTimeOffset startedAtUtc,
        DateTimeOffset completedAtUtc,
        IReadOnlyList<MonthlyMeasurement> measurements,
        BenchmarkStatistics statistics,
        string schemaVersion = "1.0")
    {
        if (id == Guid.Empty)
        {
            throw new DomainException("Benchmark result id cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Benchmark result name is required.");
        }

        if (completedAtUtc < startedAtUtc)
        {
            throw new DomainException("Benchmark result completion timestamp cannot be earlier than start timestamp.");
        }

        if (measurements.Count == 0)
        {
            throw new DomainException("Benchmark result requires at least one measurement.");
        }

        if (string.IsNullOrWhiteSpace(schemaVersion))
        {
            throw new DomainException("Benchmark result schema version is required.");
        }

        Id = id;
        Name = name.Trim();
        StartedAtUtc = startedAtUtc;
        CompletedAtUtc = completedAtUtc;
        _measurements = measurements.ToArray();
        Statistics = statistics;
        SchemaVersion = schemaVersion;
    }

    public Guid Id { get; }

    public string Name { get; }

    public string SchemaVersion { get; }

    public DateTimeOffset StartedAtUtc { get; }

    public DateTimeOffset CompletedAtUtc { get; }

    public TimeSpan Duration => CompletedAtUtc - StartedAtUtc;

    public IReadOnlyList<MonthlyMeasurement> Measurements => _measurements;

    public BenchmarkStatistics Statistics { get; }
}
