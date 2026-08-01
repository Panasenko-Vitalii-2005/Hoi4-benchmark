using HOI4Benchmark.Domain.Settings;

namespace HOI4Benchmark.Application.Abstractions;

public interface ISettingsRepository
{
    Task<BenchmarkSettings> LoadAsync(
        CancellationToken cancellationToken = default);

    Task SaveAsync(
        BenchmarkSettings settings,
        CancellationToken cancellationToken = default);

    Task ResetAsync(
        CancellationToken cancellationToken = default);
}