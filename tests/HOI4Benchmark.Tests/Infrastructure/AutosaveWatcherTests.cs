using HOI4Benchmark.Infrastructure.FileSystem;

namespace HOI4Benchmark.Tests.Infrastructure;

public sealed class AutosaveWatcherTests
{
    [Fact]
    public async Task Watcher_ShouldReportStableAutosave()
    {
        var directory = CreateTemporaryDirectory();
        var autosavePath = Path.Combine(
            directory,
            "autosave_temp.hoi4");

        var options = new AutosaveWatcherOptions
        {
            DebounceDelay = TimeSpan.FromMilliseconds(100),
            StabilityCheckInterval = TimeSpan.FromMilliseconds(50),
            RequiredStableChecks = 2,
            MaxStabilityChecks = 20,
            MaxRetryAttempts = 3,
            RetryDelay = TimeSpan.FromMilliseconds(50)
        };

        await using var watcher = new AutosaveWatcher(options);

        var completion =
            new TaskCompletionSource<string>(
                TaskCreationOptions.RunContinuationsAsynchronously);

        await watcher.StartAsync(
            autosavePath,
            (path, _) =>
            {
                completion.TrySetResult(path);
                return Task.CompletedTask;
            });

        await File.WriteAllTextAsync(
            autosavePath,
            "first chunk");

        await File.AppendAllTextAsync(
            autosavePath,
            "\nsecond chunk");

        var completedTask = await Task.WhenAny(
            completion.Task,
            Task.Delay(TimeSpan.FromSeconds(5)));

        Assert.Same(completion.Task, completedTask);

        var detectedPath = await completion.Task;

        Assert.Equal(
            Path.GetFullPath(autosavePath),
            Path.GetFullPath(detectedPath));

        Directory.Delete(directory, recursive: true);
    }

    [Fact]
    public async Task Watcher_ShouldIgnoreOtherFiles()
    {
        var directory = CreateTemporaryDirectory();
        var autosavePath = Path.Combine(
            directory,
            "autosave_temp.hoi4");

        var otherPath = Path.Combine(
            directory,
            "other_file.txt");

        var options = new AutosaveWatcherOptions
        {
            DebounceDelay = TimeSpan.FromMilliseconds(50),
            StabilityCheckInterval = TimeSpan.FromMilliseconds(50),
            RequiredStableChecks = 2,
            MaxStabilityChecks = 10
        };

        await using var watcher = new AutosaveWatcher(options);

        var callbackInvoked = false;

        await watcher.StartAsync(
            autosavePath,
            (_, _) =>
            {
                callbackInvoked = true;
                return Task.CompletedTask;
            });

        await File.WriteAllTextAsync(
            otherPath,
            "irrelevant file");

        await Task.Delay(500);

        Assert.False(callbackInvoked);

        Directory.Delete(directory, recursive: true);
    }

    private static string CreateTemporaryDirectory()
    {
        var path = Path.Combine(
            Path.GetTempPath(),
            "HOI4Benchmark.Tests",
            Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(path);

        return path;
    }
}