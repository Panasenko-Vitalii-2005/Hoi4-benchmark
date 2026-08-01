using HOI4Benchmark.Application.Exceptions;
using HOI4Benchmark.Domain.Settings;
using HOI4Benchmark.Infrastructure.Settings;

namespace HOI4Benchmark.Tests.Infrastructure;

public sealed class JsonSettingsRepositoryTests
{
    [Fact]
    public async Task LoadAsync_ShouldCreateDefaultFile_WhenFileDoesNotExist()
    {
        var filePath = CreateTemporarySettingsPath();

        try
        {
            using var repository =
                CreateRepository(filePath);

            var settings =
                await repository.LoadAsync();

            Assert.True(File.Exists(filePath));
            Assert.Equal(
                120,
                settings.TargetMeasuredMonths);
            Assert.Equal(
                12,
                settings.WarmupMonths);
        }
        finally
        {
            DeleteDirectory(filePath);
        }
    }

    [Fact]
    public async Task SaveAsync_ShouldPersistSettings()
    {
        var filePath = CreateTemporarySettingsPath();

        try
        {
            using (var repository =
                   CreateRepository(filePath))
            {
                var settings = new BenchmarkSettings
                {
                    GamePath = @"C:\Games\HOI4",
                    SavePath = @"C:\Saves\autosave_temp.hoi4",
                    TargetMeasuredMonths = 240,
                    WarmupMonths = 24
                };

                await repository.SaveAsync(settings);
            }

            using var secondRepository =
                CreateRepository(filePath);

            var loaded =
                await secondRepository.LoadAsync();

            Assert.Equal(
                @"C:\Games\HOI4",
                loaded.GamePath);

            Assert.Equal(
                @"C:\Saves\autosave_temp.hoi4",
                loaded.SavePath);

            Assert.Equal(
                240,
                loaded.TargetMeasuredMonths);

            Assert.Equal(
                24,
                loaded.WarmupMonths);
        }
        finally
        {
            DeleteDirectory(filePath);
        }
    }

    [Fact]
    public async Task ResetAsync_ShouldRestoreDefaults()
    {
        var filePath = CreateTemporarySettingsPath();

        try
        {
            using var repository =
                CreateRepository(filePath);

            await repository.SaveAsync(
                new BenchmarkSettings
                {
                    TargetMeasuredMonths = 500,
                    WarmupMonths = 50
                });

            await repository.ResetAsync();

            var loaded =
                await repository.LoadAsync();

            Assert.Equal(
                120,
                loaded.TargetMeasuredMonths);

            Assert.Equal(
                12,
                loaded.WarmupMonths);
        }
        finally
        {
            DeleteDirectory(filePath);
        }
    }

    [Fact]
    public async Task LoadAsync_ShouldThrow_WhenJsonIsInvalid()
    {
        var filePath = CreateTemporarySettingsPath();

        try
        {
            var directory =
                Path.GetDirectoryName(filePath)!;

            Directory.CreateDirectory(directory);

            await File.WriteAllTextAsync(
                filePath,
                "{ this is not valid json");

            using var repository =
                CreateRepository(filePath);

            await Assert.ThrowsAsync<
                SettingsRepositoryException>(
                () => repository.LoadAsync());
        }
        finally
        {
            DeleteDirectory(filePath);
        }
    }

    [Fact]
    public async Task SaveAsync_ShouldUseCamelCaseJson()
    {
        var filePath = CreateTemporarySettingsPath();

        try
        {
            using var repository =
                CreateRepository(filePath);

            await repository.SaveAsync(
                new BenchmarkSettings
                {
                    TargetMeasuredMonths = 120,
                    WarmupMonths = 12
                });

            var json =
                await File.ReadAllTextAsync(filePath);

            Assert.Contains(
                "\"targetMeasuredMonths\"",
                json);

            Assert.Contains(
                "\"warmupMonths\"",
                json);
        }
        finally
        {
            DeleteDirectory(filePath);
        }
    }

    private static JsonSettingsRepository CreateRepository(
        string filePath)
    {
        return new JsonSettingsRepository(
            new JsonSettingsRepositoryOptions
            {
                FilePath = filePath
            });
    }

    private static string CreateTemporarySettingsPath()
    {
        return Path.Combine(
            Path.GetTempPath(),
            "HOI4Benchmark.Tests",
            Guid.NewGuid().ToString("N"),
            "settings.json");
    }

    private static void DeleteDirectory(
        string filePath)
    {
        var directory =
            Path.GetDirectoryName(filePath);

        if (directory is not null &&
            Directory.Exists(directory))
        {
            Directory.Delete(
                directory,
                recursive: true);
        }
    }
}