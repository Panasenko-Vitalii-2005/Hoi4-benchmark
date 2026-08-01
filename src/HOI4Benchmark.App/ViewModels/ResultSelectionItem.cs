using HOI4Benchmark.Domain.Benchmarks;

namespace HOI4Benchmark.App.ViewModels;

public sealed class ResultSelectionItem
{
    public required BenchmarkResult Result { get; init; }

    public Guid Id => Result.Id;

    public string DisplayName =>
        $"{Result.CompletedAtUtc.ToLocalTime():dd.MM.yyyy HH:mm} — " +
        $"Score: {Result.Statistics.Score.Value:0}";
}