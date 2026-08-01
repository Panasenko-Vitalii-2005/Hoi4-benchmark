namespace HOI4Benchmark.Application.Abstractions;

public interface IAppLogger : IApplicationService
{
    string CurrentLogFilePath { get; }

    Task LogInformationAsync(
        string message,
        CancellationToken cancellationToken = default);

    Task LogWarningAsync(
        string message,
        CancellationToken cancellationToken = default);

    Task LogErrorAsync(
        Exception exception,
        string message,
        CancellationToken cancellationToken = default);
}