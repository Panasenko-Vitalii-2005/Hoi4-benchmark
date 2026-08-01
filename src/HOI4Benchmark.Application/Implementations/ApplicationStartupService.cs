using HOI4Benchmark.Application.Abstractions;

namespace HOI4Benchmark.Application.Implementations;

public class ApplicationStartupService : IApplicationStartupService
{
    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public Task EnsureConfigurationAsync()
    {
        return Task.CompletedTask;
    }
}