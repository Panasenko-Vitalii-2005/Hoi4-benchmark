namespace HOI4Benchmark.App.ViewModels;

public sealed class BenchmarkMeasurementItem
{
    public required string GameDate { get; init; }

    public required string Duration { get; init; }

    public required string Score { get; init; }
}