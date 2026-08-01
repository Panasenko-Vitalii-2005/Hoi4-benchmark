namespace HOI4Benchmark.Application.Abstractions;

/// <summary>
/// Watches a HOI4 autosave file and reports when the file is ready for reading.
/// </summary>
public interface IAutosaveWatcher : IAsyncDisposable
{
    bool IsRunning { get; }

    Task StartAsync(
        string autosavePath,
        Func<string, CancellationToken, Task> onAutosaveReady,
        CancellationToken cancellationToken = default);

    Task StopAsync();
}