using System.Text.Json;
using HOI4Benchmark.Application.Abstractions;
using HOI4Benchmark.Application.Exceptions;
using HOI4Benchmark.Domain.Benchmarks;

namespace HOI4Benchmark.Infrastructure.Results;

public sealed class JsonResultRepository :
    IRepository<BenchmarkResult>,
    IDisposable
{
    private static readonly JsonSerializerOptions SerializerOptions =
        new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

    private readonly string _filePath;
    private readonly SemaphoreSlim _fileLock = new(1, 1);

    private bool _disposed;

    public JsonResultRepository()
        : this(new JsonResultRepositoryOptions())
    {
    }

    public JsonResultRepository(
        JsonResultRepositoryOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (string.IsNullOrWhiteSpace(options.FilePath))
        {
            throw new ArgumentException(
                "Result file path cannot be empty.",
                nameof(options));
        }

        _filePath = Path.GetFullPath(options.FilePath);
    }

    public async Task AddAsync(
        BenchmarkResult entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        ThrowIfDisposed();

        await _fileLock.WaitAsync();

        try
        {
            var document =
                await LoadDocumentInternalAsync(
                    CancellationToken.None);

            var alreadyExists = document.Results.Any(
                result => result.Id == entity.Id);

            if (alreadyExists)
            {
                throw new ResultRepositoryException(
                    $"Benchmark result with ID " +
                    $"'{entity.Id}' already exists.");
            }

            document.Results.Add(entity);

            await SaveDocumentInternalAsync(
                document,
                CancellationToken.None);
        }
        catch (ResultRepositoryException)
        {
            throw;
        }
        catch (IOException exception)
        {
            throw CreateWriteException(exception);
        }
        catch (UnauthorizedAccessException exception)
        {
            throw CreateAccessException(exception);
        }
        finally
        {
            _fileLock.Release();
        }
    }

    public async Task UpdateAsync(
        BenchmarkResult entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        ThrowIfDisposed();

        await _fileLock.WaitAsync();

        try
        {
            var document =
                await LoadDocumentInternalAsync(
                    CancellationToken.None);

            var index = document.Results.FindIndex(
                result => result.Id == entity.Id);

            if (index < 0)
            {
                throw new ResultRepositoryException(
                    $"Benchmark result with ID " +
                    $"'{entity.Id}' was not found.");
            }

            document.Results[index] = entity;

            await SaveDocumentInternalAsync(
                document,
                CancellationToken.None);
        }
        catch (ResultRepositoryException)
        {
            throw;
        }
        catch (IOException exception)
        {
            throw CreateWriteException(exception);
        }
        catch (UnauthorizedAccessException exception)
        {
            throw CreateAccessException(exception);
        }
        finally
        {
            _fileLock.Release();
        }
    }

    public async Task DeleteAsync(
        BenchmarkResult entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        ThrowIfDisposed();

        await _fileLock.WaitAsync();

        try
        {
            var document =
                await LoadDocumentInternalAsync(
                    CancellationToken.None);

            var removedCount = document.Results.RemoveAll(
                result => result.Id == entity.Id);

            if (removedCount == 0)
            {
                return;
            }

            await SaveDocumentInternalAsync(
                document,
                CancellationToken.None);
        }
        catch (ResultRepositoryException)
        {
            throw;
        }
        catch (IOException exception)
        {
            throw CreateWriteException(exception);
        }
        catch (UnauthorizedAccessException exception)
        {
            throw CreateAccessException(exception);
        }
        finally
        {
            _fileLock.Release();
        }
    }

    public async Task<BenchmarkResult?> GetByIdAsync(
        Guid id)
    {
        ThrowIfDisposed();

        await _fileLock.WaitAsync();

        try
        {
            var document =
                await LoadDocumentInternalAsync(
                    CancellationToken.None);

            return document.Results.FirstOrDefault(
                result => result.Id == id);
        }
        catch (ResultRepositoryException)
        {
            throw;
        }
        catch (IOException exception)
        {
            throw CreateReadException(exception);
        }
        catch (UnauthorizedAccessException exception)
        {
            throw CreateAccessException(exception);
        }
        finally
        {
            _fileLock.Release();
        }
    }

    public async Task<IEnumerable<BenchmarkResult>> GetAllAsync()
{
    ThrowIfDisposed();

    await _fileLock.WaitAsync();

    try
    {
        var document =
            await LoadDocumentInternalAsync(
                CancellationToken.None);

        return document.Results.ToList();
    }
    catch (ResultRepositoryException)
    {
        throw;
    }
    catch (IOException exception)
    {
        throw CreateReadException(exception);
    }
    catch (UnauthorizedAccessException exception)
    {
        throw CreateAccessException(exception);
    }
    finally
    {
        _fileLock.Release();
    }
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

    private async Task<BenchmarkResultsDocument>
        LoadDocumentInternalAsync(
            CancellationToken cancellationToken)
    {
        if (!File.Exists(_filePath))
        {
            return new BenchmarkResultsDocument();
        }

        await using var stream = new FileStream(
            _filePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 8192,
            FileOptions.Asynchronous |
            FileOptions.SequentialScan);

        try
        {
            var document =
                await JsonSerializer.DeserializeAsync<
                    BenchmarkResultsDocument>(
                    stream,
                    SerializerOptions,
                    cancellationToken);

            if (document is null)
            {
                throw new ResultRepositoryException(
                    $"Result file is empty or invalid: " +
                    $"{_filePath}");
            }

            if (document.Version != 1)
            {
                throw new ResultRepositoryException(
                    $"Unsupported result file version " +
                    $"'{document.Version}': {_filePath}");
            }

            return document;
        }
        catch (JsonException exception)
        {
            throw new ResultRepositoryException(
                $"Result file contains invalid JSON: " +
                $"{_filePath}",
                exception);
        }
    }

    private async Task SaveDocumentInternalAsync(
        BenchmarkResultsDocument document,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(document);

        var directoryPath =
            Path.GetDirectoryName(_filePath);

        if (string.IsNullOrWhiteSpace(directoryPath))
        {
            throw new ResultRepositoryException(
                $"Result path has no valid directory: " +
                $"{_filePath}");
        }

        Directory.CreateDirectory(directoryPath);

        var temporaryPath = _filePath + ".tmp";

        try
        {
            await using (var stream = new FileStream(
                temporaryPath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 8192,
                FileOptions.Asynchronous |
                FileOptions.WriteThrough))
            {
                await JsonSerializer.SerializeAsync(
                    stream,
                    document,
                    SerializerOptions,
                    cancellationToken);

                await stream.FlushAsync(
                    cancellationToken);
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

    private ResultRepositoryException CreateReadException(
        Exception exception)
    {
        return new ResultRepositoryException(
            $"Could not read benchmark results: " +
            $"{_filePath}",
            exception);
    }

    private ResultRepositoryException CreateWriteException(
        Exception exception)
    {
        return new ResultRepositoryException(
            $"Could not save benchmark results: " +
            $"{_filePath}",
            exception);
    }

    private ResultRepositoryException CreateAccessException(
        Exception exception)
    {
        return new ResultRepositoryException(
            $"Access to benchmark results was denied: " +
            $"{_filePath}",
            exception);
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(
            _disposed,
            this);
    }
}   