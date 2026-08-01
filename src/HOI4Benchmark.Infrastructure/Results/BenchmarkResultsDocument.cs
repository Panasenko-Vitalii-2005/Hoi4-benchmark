using HOI4Benchmark.Domain.Benchmarks;

namespace HOI4Benchmark.Infrastructure.Results;

public sealed class BenchmarkResultsDocument
{
    public int Version { get; init; } = 1;

    public List<BenchmarkResult> Results { get; init; } = [];
}