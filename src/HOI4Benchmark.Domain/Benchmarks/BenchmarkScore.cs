using HOI4Benchmark.Domain.Common;

namespace HOI4Benchmark.Domain.Benchmarks;

public sealed record BenchmarkScore
{
    public BenchmarkScore(decimal value, string formulaVersion, decimal baselineMonthTimeSeconds, string? description = null)
    {
        if (value < 0)
        {
            throw new DomainException("Benchmark score cannot be negative.");
        }

        if (string.IsNullOrWhiteSpace(formulaVersion))
        {
            throw new DomainException("Benchmark score formula version is required.");
        }

        if (baselineMonthTimeSeconds <= 0)
        {
            throw new DomainException("Benchmark score baseline month time must be greater than zero.");
        }

        Value = value;
        FormulaVersion = formulaVersion;
        BaselineMonthTimeSeconds = baselineMonthTimeSeconds;
        Description = description;
    }

    public decimal Value { get; }

    public string FormulaVersion { get; }

    public decimal BaselineMonthTimeSeconds { get; }

    public string? Description { get; }
}
