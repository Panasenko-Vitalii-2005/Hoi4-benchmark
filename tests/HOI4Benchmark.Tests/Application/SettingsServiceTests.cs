using HOI4Benchmark.Application.Implementations;
using HOI4Benchmark.Domain.Settings;
using HOI4Benchmark.Tests.Fakes;

namespace HOI4Benchmark.Tests.Application;

public class SettingsServiceTests
{
    [Fact]
    public async Task UpdateSettings_ShouldChangeSettings()
    {
        var repository = new FakeSettingsRepository();
        var service = new SettingsService(repository);

        var settings = new BenchmarkSettings
        {
            TargetMeasuredMonths = 240
        };

        await service.UpdateSettingsAsync(settings);

        var result = await service.GetSettingsAsync();

        Assert.Equal(
            240,
            result.TargetMeasuredMonths);
    }

    [Fact]
    public async Task ResetSettings_ShouldRestoreDefaults()
    {
        var repository = new FakeSettingsRepository();
        var service = new SettingsService(repository);

        var customSettings = new BenchmarkSettings
        {
            TargetMeasuredMonths = 240
        };

        await service.UpdateSettingsAsync(customSettings);
        await service.ResetSettingsAsync();

        var result = await service.GetSettingsAsync();

        Assert.Equal(
            120,
            result.TargetMeasuredMonths);
    }
}