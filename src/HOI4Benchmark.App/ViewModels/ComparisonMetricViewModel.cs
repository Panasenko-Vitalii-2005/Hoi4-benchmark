namespace HOI4Benchmark.App.ViewModels;

public sealed class ComparisonMetricViewModel
{
    public required string Name { get; init; }

    public required string FirstValue { get; init; }

    public required string SecondValue { get; init; }

    public required string Difference { get; init; }

    public bool IsFirstBetter { get; init; }

    public bool IsSecondBetter { get; init; }

    public bool IsEqual => !IsFirstBetter && !IsSecondBetter;
}