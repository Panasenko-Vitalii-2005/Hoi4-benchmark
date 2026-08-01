using System.Text.Json;
using HOI4Benchmark.Application.Abstractions;
using HOI4Benchmark.Application.Exceptions;
using HOI4Benchmark.Domain.Settings;

namespace HOI4Benchmark.Infrastructure.Settings;

public sealed class JsonSettingsRepository
    : ISettingsRepository, IDisposable
{
    private static readonly JsonSerializerOptions SerializerOptions =
        new()
        {
            WriteIndented = true,
            PropertyNamingPolicy =
                JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

    private readonly string _filePath;
    private readonly SemaphoreSlim _fileLock = new(1, 1);

    private bool _disposed;

    public JsonSettingsRepository()
        : this(new JsonSettingsRepositoryOptions())
    {
    }

    public JsonSettingsRepository(
        JsonSettingsRepositoryOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (string.IsNullOrWhiteSpace(options.FilePath))
        {
            throw new ArgumentException(
                "Settings file path cannot be empty.",
                nameof(options));
        }

        _filePath = Path.GetFullPath(options.FilePath);
    }

    public async Task<BenchmarkSettings> LoadAsync(
        CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        await _fileLock.WaitAsync(cancellationToken);

        try
        {
            if (!File.Exists(_filePath))
            {
                var defaultSettings =
                    CreateDefaultSettings();

                await SaveInternalAsync(
                    defaultSettings,
                    cancellationToken);

                return defaultSettings;
            }

            await using var stream = new FileStream(
                _filePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: 4096,
                FileOptions.Asynchronous |
                FileOptions.SequentialScan);

            BenchmarkSettingsDocument? document;

            try
            {
                document =
                    await JsonSerializer.DeserializeAsync<
                        BenchmarkSettingsDocument>(
                        stream,
                        SerializerOptions,
                        cancellationToken);
            }
            catch (JsonException exception)
            {
                throw new SettingsRepositoryException(
                    $"Settings file contains invalid JSON: {_filePath}",
                    exception);
            }

            if (document is null)
            {
                throw new SettingsRepositoryException(
                    $"Settings file is empty or invalid: {_filePath}");
            }

            if (document.Version != 1)
            {
                throw new SettingsRepositoryException(
                    $"Unsupported settings version " +
                    $"{document.Version} in file: {_filePath}");
            }

            return document.ToDomain();
        }
        catch (SettingsRepositoryException)
        {
            throw;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (IOException exception)
        {
            throw new SettingsRepositoryException(
                $"Could not read settings file: {_filePath}",
                exception);
        }
        catch (UnauthorizedAccessException exception)
        {
            throw new SettingsRepositoryException(
                $"Access to settings file was denied: {_filePath}",
                exception);
        }
        finally
        {
            _fileLock.Release();
        }
    }

    public async Task SaveAsync(
        BenchmarkSettings settings,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ThrowIfDisposed();

        await _fileLock.WaitAsync(cancellationToken);

        try
        {
            await SaveInternalAsync(
                settings,
                cancellationToken);
        }
        catch (SettingsRepositoryException)
        {
            throw;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (IOException exception)
        {
            throw new SettingsRepositoryException(
                $"Could not save settings file: {_filePath}",
                exception);
        }
        catch (UnauthorizedAccessException exception)
        {
            throw new SettingsRepositoryException(
                $"Access to settings file was denied: {_filePath}",
                exception);
        }
        finally
        {
            _fileLock.Release();
        }
    }

    public async Task ResetAsync(
        CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        await SaveAsync(
            CreateDefaultSettings(),
            cancellationToken);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _fileLock.Dispose();
        _disposed = true;
    }

    private async Task SaveInternalAsync(
        BenchmarkSettings settings,
        CancellationToken cancellationToken)
    {
        var directoryPath =
            Path.GetDirectoryName(_filePath);

        if (string.IsNullOrWhiteSpace(directoryPath))
        {
            throw new SettingsRepositoryException(
                $"Settings path has no valid directory: {_filePath}");
        }

        Directory.CreateDirectory(directoryPath);

        var temporaryPath = _filePath + ".tmp";

        var document =
            BenchmarkSettingsDocument.FromDomain(settings);

        try
        {
            await using (var stream = new FileStream(
                temporaryPath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 4096,
                FileOptions.Asynchronous |
                FileOptions.WriteThrough))
            {
                await JsonSerializer.SerializeAsync<
    BenchmarkSettingsDocument>(
    stream,
    document,
    SerializerOptions,
    cancellationToken);

                await stream.FlushAsync(cancellationToken);
            }

            File.Move(
                temporaryPath,
                _filePath,
                overwrite: true);
        }
        finally
        {
            if (File.Exists(temporaryPath))
            {
                File.Delete(temporaryPath);
            }
        }
    }

    private static BenchmarkSettings CreateDefaultSettings()
    {
        return new BenchmarkSettings
        {
            GamePath = string.Empty,
            SavePath = string.Empty,
            TargetMeasuredMonths = 120,
            WarmupMonths = 12
        };
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(
            _disposed,
            this);
    }
}