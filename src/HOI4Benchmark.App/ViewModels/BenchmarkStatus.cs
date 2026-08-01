namespace HOI4Benchmark.App.ViewModels;

public enum BenchmarkStatus
{
    Idle,
    WaitingForAutosave,
    Running,
    Completed,
    Cancelled,
    Failed
}