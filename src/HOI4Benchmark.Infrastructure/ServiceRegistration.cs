using HOI4Benchmark.Application.Abstractions;
using HOI4Benchmark.Domain.Benchmarks;
using HOI4Benchmark.Infrastructure.FileSystem;
using HOI4Benchmark.Infrastructure.Parsing;
using HOI4Benchmark.Infrastructure.Results;
using HOI4Benchmark.Infrastructure.Settings;
using HOI4Benchmark.Infrastructure.SystemInformation;
using HOI4Benchmark.Infrastructure.Diagnostics;
using HOI4Benchmark.Infrastructure.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace HOI4Benchmark.Infrastructure;

public static class ServiceRegistration
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services)
    {
        services.AddSingleton<
            IAutosaveWatcher,
            AutosaveWatcher>();

        services.AddSingleton<
            IInitialSaveDateParser,
            InitialSaveDateParser>();

        services.AddSingleton(
            new JsonSettingsRepositoryOptions());

        services.AddSingleton<
            ISettingsRepository,
            JsonSettingsRepository>();

        services.AddSingleton(
            new JsonResultRepositoryOptions());

        services.AddSingleton<
            IRepository<BenchmarkResult>,
            JsonResultRepository>();

        if (OperatingSystem.IsWindows())
        {
            services.AddSingleton<
                ISystemInformationProvider,
                WindowsSystemInformationProvider>();
        }

        services.AddSingleton<
    IAppLogger,
    FileAppLogger>();

services.AddTransient<
    IDiagnosticsBundleService,
    DiagnosticsBundleService>();

        return services;
    }
}