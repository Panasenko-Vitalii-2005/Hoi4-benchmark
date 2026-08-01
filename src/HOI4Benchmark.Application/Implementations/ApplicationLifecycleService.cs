// src/HOI4Benchmark.Application/Implementations/ApplicationLifecycleService.cs
using HOI4Benchmark.Application.Abstractions;
using System.Threading.Tasks;

namespace HOI4Benchmark.Application.Implementations;

public class ApplicationLifecycleService : IApplicationLifecycleService
{
    public Task StartAsync()
    {
        return Task.CompletedTask;
    }

    public Task StopAsync()
    {
        return Task.CompletedTask;
    }
}