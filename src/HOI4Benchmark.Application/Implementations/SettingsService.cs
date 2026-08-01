using HOI4Benchmark.Application.Abstractions;
using HOI4Benchmark.Domain.Settings;

namespace HOI4Benchmark.Application.Implementations;

public sealed class SettingsService : ISettingsService
{
    private readonly ISettingsRepository _repository;

    public SettingsService(
        ISettingsRepository repository)
    {
        _repository = repository
            ?? throw new ArgumentNullException(
                nameof(repository));
    }

    public Task<BenchmarkSettings> GetSettingsAsync()
    {
        return _repository.LoadAsync();
    }

    public Task UpdateSettingsAsync(
        BenchmarkSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        return _repository.SaveAsync(settings);
    }

    public Task ResetSettingsAsync()
    {
        return _repository.ResetAsync();
    }
}