using HOI4Benchmark.Domain.Settings;

namespace HOI4Benchmark.Application.Abstractions;

public interface ISettingsService : IApplicationService
{
    Task<BenchmarkSettings> GetSettingsAsync();

    Task UpdateSettingsAsync(BenchmarkSettings settings);

    Task ResetSettingsAsync();
}