namespace HOI4Benchmark.Application.Abstractions;

public interface IDiagnosticsBundleService : IApplicationService
{
    Task CreateBundleAsync(
        string destinationPath,
        CancellationToken cancellationToken = default);
}