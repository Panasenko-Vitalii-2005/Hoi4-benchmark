using HOI4Benchmark.Application.Abstractions;

namespace HOI4Benchmark.Infrastructure.FileSystem;

public sealed class AutosaveWatcher : IAutosaveWatcher
{
    private readonly AutosaveWatcherOptions _options;
    private readonly object _syncRoot = new();

    private FileSystemWatcher? _watcher;
    private CancellationTokenSource? _lifetimeCancellation;
    private CancellationTokenSource? _debounceCancellation;

    private Func<string, CancellationToken, Task>? _onAutosaveReady;
    private string? _autosavePath;

    public AutosaveWatcher()
        : this(new AutosaveWatcherOptions())
    {
    }

    public AutosaveWatcher(AutosaveWatcherOptions options)
    {
        _options = options
            ?? throw new ArgumentNullException(nameof(options));

        _options.Validate();
    }

    public bool IsRunning
    {
        get
        {
            lock (_syncRoot)
            {
                return _watcher is not null;
            }
        }
    }

    public Task StartAsync(
        string autosavePath,
        Func<string, CancellationToken, Task> onAutosaveReady,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(autosavePath);
        ArgumentNullException.ThrowIfNull(onAutosaveReady);

        var fullPath = Path.GetFullPath(autosavePath);
        var directoryPath = Path.GetDirectoryName(fullPath);
        var fileName = Path.GetFileName(fullPath);

        if (string.IsNullOrWhiteSpace(directoryPath))
        {
            throw new ArgumentException(
                "Autosave path must contain a directory.",
                nameof(autosavePath));
        }

        if (!Directory.Exists(directoryPath))
        {
            throw new DirectoryNotFoundException(
                $"Autosave directory was not found: {directoryPath}");
        }

        lock (_syncRoot)
        {
            if (_watcher is not null)
            {
                throw new InvalidOperationException(
                    "Autosave watcher is already running.");
            }

            _autosavePath = fullPath;
            _onAutosaveReady = onAutosaveReady;

            _lifetimeCancellation =
                CancellationTokenSource.CreateLinkedTokenSource(
                    cancellationToken);

            _watcher = new FileSystemWatcher(directoryPath, fileName)
            {
                IncludeSubdirectories = false,
                NotifyFilter =
                    NotifyFilters.FileName |
                    NotifyFilters.CreationTime |
                    NotifyFilters.LastWrite |
                    NotifyFilters.Size
            };

            _watcher.Created += HandleFileChanged;
            _watcher.Changed += HandleFileChanged;
            _watcher.Renamed += HandleFileRenamed;
            _watcher.Error += HandleWatcherError;

            _watcher.EnableRaisingEvents = true;
        }

        return Task.CompletedTask;
    }

    public Task StopAsync()
    {
        FileSystemWatcher? watcher;
        CancellationTokenSource? lifetimeCancellation;
        CancellationTokenSource? debounceCancellation;

        lock (_syncRoot)
        {
            watcher = _watcher;
            lifetimeCancellation = _lifetimeCancellation;
            debounceCancellation = _debounceCancellation;

            _watcher = null;
            _lifetimeCancellation = null;
            _debounceCancellation = null;
            _onAutosaveReady = null;
            _autosavePath = null;
        }

        debounceCancellation?.Cancel();
        lifetimeCancellation?.Cancel();

        if (watcher is not null)
        {
            watcher.EnableRaisingEvents = false;

            watcher.Created -= HandleFileChanged;
            watcher.Changed -= HandleFileChanged;
            watcher.Renamed -= HandleFileRenamed;
            watcher.Error -= HandleWatcherError;

            watcher.Dispose();
        }

        debounceCancellation?.Dispose();
        lifetimeCancellation?.Dispose();

        return Task.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync();
    }

    private void HandleFileChanged(
        object sender,
        FileSystemEventArgs eventArgs)
    {
        ScheduleProcessing(eventArgs.FullPath);
    }

    private void HandleFileRenamed(
        object sender,
        RenamedEventArgs eventArgs)
    {
        ScheduleProcessing(eventArgs.FullPath);
    }

    private void HandleWatcherError(
        object sender,
        ErrorEventArgs eventArgs)
    {
        // FileSystemWatcher can lose events if its internal buffer overflows.
        // Scheduling another check gives the target file a chance to be processed.
        string? autosavePath;

        lock (_syncRoot)
        {
            autosavePath = _autosavePath;
        }

        if (autosavePath is not null)
        {
            ScheduleProcessing(autosavePath);
        }
    }

    private void ScheduleProcessing(string changedPath)
    {
        string? autosavePath;
        CancellationToken lifetimeToken;

        lock (_syncRoot)
        {
            autosavePath = _autosavePath;

            if (autosavePath is null ||
                _lifetimeCancellation is null)
            {
                return;
            }

            if (!PathsAreEqual(changedPath, autosavePath))
            {
                return;
            }

            _debounceCancellation?.Cancel();
            _debounceCancellation?.Dispose();

            _debounceCancellation =
                CancellationTokenSource.CreateLinkedTokenSource(
                    _lifetimeCancellation.Token);

            lifetimeToken = _debounceCancellation.Token;
        }

        _ = ProcessAutosaveAsync(autosavePath, lifetimeToken);
    }

    private async Task ProcessAutosaveAsync(
        string autosavePath,
        CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(
                _options.DebounceDelay,
                cancellationToken);

            for (var attempt = 1;
                 attempt <= _options.MaxRetryAttempts;
                 attempt++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    var isStable = await WaitUntilStableAsync(
                        autosavePath,
                        cancellationToken);

                    if (!isStable)
                    {
                        throw new IOException(
                            $"Autosave did not become stable: {autosavePath}");
                    }

                    Func<string, CancellationToken, Task>? callback;

                    lock (_syncRoot)
                    {
                        callback = _onAutosaveReady;
                    }

                    if (callback is not null)
                    {
                        await callback(
                            autosavePath,
                            cancellationToken);
                    }

                    return;
                }
                catch (Exception exception)
                    when (IsRetryable(exception) &&
                          attempt < _options.MaxRetryAttempts)
                {
                    await Task.Delay(
                        _options.RetryDelay,
                        cancellationToken);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Normal when another file event resets debounce
            // or when the watcher is stopped.
        }
    }

    private async Task<bool> WaitUntilStableAsync(
        string path,
        CancellationToken cancellationToken)
    {
        long? previousLength = null;
        DateTime? previousWriteTimeUtc = null;
        var stableChecks = 0;

        for (var check = 0;
             check < _options.MaxStabilityChecks;
             check++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!File.Exists(path))
            {
                stableChecks = 0;

                await Task.Delay(
                    _options.StabilityCheckInterval,
                    cancellationToken);

                continue;
            }

            var fileInfo = new FileInfo(path);
            fileInfo.Refresh();

            var length = fileInfo.Length;
            var writeTimeUtc = fileInfo.LastWriteTimeUtc;

            var metadataUnchanged =
                previousLength == length &&
                previousWriteTimeUtc == writeTimeUtc;

            var canOpenExclusively =
                metadataUnchanged &&
                CanOpenExclusively(path);

            if (canOpenExclusively)
            {
                stableChecks++;

                if (stableChecks >= _options.RequiredStableChecks)
                {
                    return true;
                }
            }
            else
            {
                stableChecks = 0;
            }

            previousLength = length;
            previousWriteTimeUtc = writeTimeUtc;

            await Task.Delay(
                _options.StabilityCheckInterval,
                cancellationToken);
        }

        return false;
    }

    private static bool CanOpenExclusively(string path)
    {
        try
        {
            using var stream = new FileStream(
                path,
                FileMode.Open,
                FileAccess.Read,
                FileShare.None);

            return stream.Length >= 0;
        }
        catch (IOException)
        {
            return false;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
    }

    private static bool IsRetryable(Exception exception)
    {
        return exception is IOException or UnauthorizedAccessException;
    }

    private static bool PathsAreEqual(
        string firstPath,
        string secondPath)
    {
        return string.Equals(
            Path.GetFullPath(firstPath),
            Path.GetFullPath(secondPath),
            StringComparison.OrdinalIgnoreCase);
    }
}