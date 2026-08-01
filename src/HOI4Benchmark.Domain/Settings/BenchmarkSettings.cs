namespace HOI4Benchmark.Domain.Settings;

public sealed class BenchmarkSettings
{
    public string GamePath { get; set; } = string.Empty;

    public string SavePath { get; set; } =
        @"Documents\Paradox Interactive\Hearts of Iron IV\save games";

    public int TargetMeasuredMonths { get; set; } = 120;

    public int WarmupMonths { get; set; }

    public int MeasurementIntervalSeconds { get; set; } = 1;

    public string DefaultExportFormat { get; set; } = "JSON";

    public bool SaveResultsAutomatically { get; set; } = true;

    public bool WatchSubdirectories { get; set; }

    public bool ShowNotifications { get; set; } = true;
}