namespace HOI4Benchmark.Infrastructure.Results;

public sealed class JsonResultRepositoryOptions
{
    public string FilePath { get; init; } =
        GetDefaultFilePath();

    private static string GetDefaultFilePath()
    {
        var localApplicationData =
            Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData);

        return Path.Combine(
            localApplicationData,
            "HOI4Benchmark",
            "results.json");
    }
}