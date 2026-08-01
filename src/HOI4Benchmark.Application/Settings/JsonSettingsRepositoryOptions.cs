namespace HOI4Benchmark.Infrastructure.Settings;

public sealed class JsonSettingsRepositoryOptions
{
    public string FilePath { get; init; } =
        GetDefaultSettingsPath();

    private static string GetDefaultSettingsPath()
    {
        var applicationDataPath =
            Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData);

        return Path.Combine(
            applicationDataPath,
            "HOI4Benchmark",
            "settings.json");
    }
}