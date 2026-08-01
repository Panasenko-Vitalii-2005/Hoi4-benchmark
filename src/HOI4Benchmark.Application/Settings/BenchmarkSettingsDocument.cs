using HOI4Benchmark.Domain.Settings;

namespace HOI4Benchmark.Infrastructure.Settings;

public sealed class BenchmarkSettingsDocument
{
    public int Version { get; init; } = 1;

    public string GamePath { get; init; } = string.Empty;

    public string SavePath { get; init; } = string.Empty;

    public int TargetMeasuredMonths { get; init; }

    public int WarmupMonths { get; init; }

    public static BenchmarkSettingsDocument FromDomain(
        BenchmarkSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        return new BenchmarkSettingsDocument
        {
            Version = 1,
            GamePath = settings.GamePath,
            SavePath = settings.SavePath,
            TargetMeasuredMonths =
                settings.TargetMeasuredMonths,
            WarmupMonths =
                settings.WarmupMonths
        };
    }

    public BenchmarkSettings ToDomain()
    {
        return new BenchmarkSettings
        {
            GamePath = GamePath,
            SavePath = SavePath,
            TargetMeasuredMonths =
                TargetMeasuredMonths,
            WarmupMonths =
                WarmupMonths
        };
    }
}