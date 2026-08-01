using HOI4Benchmark.Application.Abstractions;
using HOI4Benchmark.Application.Implementations;
using HOI4Benchmark.Infrastructure.Export;
using Microsoft.Extensions.DependencyInjection;

namespace HOI4Benchmark.App.DependencyInjection;

public static class ServiceRegistration
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services)
    {
        services.AddScoped<
            IBenchmarkSessionService,
            BenchmarkSessionService>();

        services.AddScoped<
            IResultService,
            ResultService>();

        services.AddScoped<
            ISettingsService,
            SettingsService>();

        services.AddSingleton<
            IExportService,
            ExportService>();

        services.AddScoped<
            IApplicationStartupService,
            ApplicationStartupService>();

        services.AddScoped<
            IApplicationLifecycleService,
            ApplicationLifecycleService>();

        return services;
    }
}