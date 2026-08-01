namespace HOI4Benchmark.Domain.Benchmarks;

public enum BenchmarkStatus
{
    NotStarted = 0,
    WaitingForAutosave = 1,
    Running = 2,
    Completed = 3,
    Cancelled = 4,
    Failed = 5,
}
