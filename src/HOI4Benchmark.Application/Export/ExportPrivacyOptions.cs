namespace HOI4Benchmark.Application.Export;

public sealed class ExportPrivacyOptions
{
    public bool AnonymizeResultNames { get; init; }

    public bool ExcludeExactTimestamps { get; init; }

    public bool ExcludeWarnings { get; init; }
}