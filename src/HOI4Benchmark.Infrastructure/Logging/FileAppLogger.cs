using System.Text;
using HOI4Benchmark.Application.Abstractions;

namespace HOI4Benchmark.Infrastructure.Logging;

public sealed class FileAppLogger : IAppLogger
{
    private readonly SemaphoreSlim _writeLock =
        new(1, 1);

    public FileAppLogger()
    {
        string applicationDataPath =
            Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData);

        string logDirectory = Path.Combine(
            applicationDataPath,
            "HOI4Benchmark",
            "logs");

        Directory.CreateDirectory(logDirectory);

        CurrentLogFilePath = Path.Combine(
            logDirectory,
            "app.log");
    }

    public string CurrentLogFilePath { get; }

    public Task LogInformationAsync(
        string message,
        CancellationToken cancellationToken = default)
    {
        return WriteAsync(
            "INFO",
            message,
            exception: null,
            cancellationToken);
    }

    public Task LogWarningAsync(
        string message,
        CancellationToken cancellationToken = default)
    {
        return WriteAsync(
            "WARNING",
            message,
            exception: null,
            cancellationToken);
    }

    public Task LogErrorAsync(
        Exception exception,
        string message,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(exception);

        return WriteAsync(
            "ERROR",
            message,
            exception,
            cancellationToken);
    }

    private async Task WriteAsync(
        string level,
        string message,
        Exception? exception,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        var builder = new StringBuilder();

        builder.Append(
            DateTimeOffset.UtcNow.ToString("O"));

        builder.Append(" [");
        builder.Append(level);
        builder.Append("] ");
        builder.AppendLine(message.Trim());

        if (exception is not null)
        {
            builder.AppendLine(exception.ToString());
        }

        await _writeLock.WaitAsync(
            cancellationToken);

        try
        {
            await File.AppendAllTextAsync(
                CurrentLogFilePath,
                builder.ToString(),
                Encoding.UTF8,
                cancellationToken);
        }
        catch
        {
            // Logging must never crash the application.
        }
        finally
        {
            _writeLock.Release();
        }
    }
}