using HOI4Benchmark.Application.Abstractions;
using HOI4Benchmark.Domain.Settings;

namespace HOI4Benchmark.Tests.Fakes;

public sealed class FakeSettingsRepository
    : ISettingsRepository
{
    private BenchmarkSettings _settings = CreateDefault();

    public Task<BenchmarkSettings> LoadAsync(
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_settings);
    }

    public Task SaveAsync(
        BenchmarkSettings settings,
        CancellationToken cancellationToken = default)
    {
        _settings = settings;
        return Task.CompletedTask;
    }

    public Task ResetAsync(
        CancellationToken cancellationToken = default)
    {
        _settings = CreateDefault();
        return Task.CompletedTask;
    }

    private static BenchmarkSettings CreateDefault()
    {
        return new BenchmarkSettings
        {
            GamePath = string.Empty,
            SavePath = string.Empty,
            TargetMeasuredMonths = 120,
            WarmupMonths = 12
        };
    }
}