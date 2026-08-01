using HOI4Benchmark.Application.Exceptions;
using HOI4Benchmark.Domain.Benchmarks;
using HOI4Benchmark.Infrastructure.Results;
using HOI4Benchmark.Domain.Game;
using System.Linq;
namespace HOI4Benchmark.Tests.Infrastructure;

public sealed class JsonResultRepositoryTests
{
    [Fact]
    public async Task AddAsync_ShouldPersistResult()
    {
        var filePath = CreateTemporaryFilePath();

        try
        {
            var result = CreateResult();

            using (var repository =
                   CreateRepository(filePath))
            {
                await repository.AddAsync(result);
            }

            using var secondRepository =
                CreateRepository(filePath);

            var loaded =
                await secondRepository.GetByIdAsync(result.Id);

            Assert.NotNull(loaded);
            Assert.Equal(result.Id, loaded.Id);
        }
        finally
        {
            DeleteTemporaryDirectory(filePath);
        }
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllResults()
    {
        var filePath = CreateTemporaryFilePath();

        try
        {
            using var repository =
                CreateRepository(filePath);

            await repository.AddAsync(CreateResult());
            await repository.AddAsync(CreateResult());

            var results =
                await repository.GetAllAsync();

            Assert.Equal(2, results.Count());
        }
        finally
        {
            DeleteTemporaryDirectory(filePath);
        }
    }

    [Fact]
    public async Task UpdateAsync_ShouldReplaceExistingResult()
    {
        var filePath = CreateTemporaryFilePath();

        try
        {
            using var repository =
                CreateRepository(filePath);

            var original = CreateResult();

            await repository.AddAsync(original);

            var updated = CreateUpdatedResult(original.Id);

            await repository.UpdateAsync(updated);

            var loaded =
                await repository.GetByIdAsync(original.Id);

            Assert.NotNull(loaded);
            Assert.Equal(updated.Id, loaded.Id);

            // Здесь добавь проверку изменённого свойства:
            // Assert.Equal(updated.Score, loaded.Score);
        }
        finally
        {
            DeleteTemporaryDirectory(filePath);
        }
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveResult()
    {
        var filePath = CreateTemporaryFilePath();

        try
        {
            using var repository =
                CreateRepository(filePath);

            var result = CreateResult();

            await repository.AddAsync(result);
            await repository.DeleteAsync(result);

            var loaded =
                await repository.GetByIdAsync(result.Id);

            Assert.Null(loaded);
        }
        finally
        {
            DeleteTemporaryDirectory(filePath);
        }
    }

    [Fact]
    public async Task AddAsync_ShouldThrow_WhenIdAlreadyExists()
    {
        var filePath = CreateTemporaryFilePath();

        try
        {
            using var repository =
                CreateRepository(filePath);

            var result = CreateResult();

            await repository.AddAsync(result);

            await Assert.ThrowsAsync<
                ResultRepositoryException>(
                () => repository.AddAsync(result));
        }
        finally
        {
            DeleteTemporaryDirectory(filePath);
        }
    }

    [Fact]
    public async Task Load_ShouldThrow_WhenJsonIsInvalid()
    {
        var filePath = CreateTemporaryFilePath();

        try
        {
            Directory.CreateDirectory(
                Path.GetDirectoryName(filePath)!);

            await File.WriteAllTextAsync(
                filePath,
                "{ invalid json");

            using var repository =
                CreateRepository(filePath);

            await Assert.ThrowsAsync<
                ResultRepositoryException>(
                () => repository.GetAllAsync());
        }
        finally
        {
            DeleteTemporaryDirectory(filePath);
        }
    }

    private static JsonResultRepository CreateRepository(
        string filePath)
    {
        return new JsonResultRepository(
            new JsonResultRepositoryOptions
            {
                FilePath = filePath
            });
    }

    private static BenchmarkResult CreateResult(
    Guid? id = null,
    string name = "Test benchmark")
{
    var startedAtUtc = new DateTimeOffset(
        2026,
        1,
        1,
        12,
        0,
        0,
        TimeSpan.Zero);

    var monthTime = TimeSpan.FromSeconds(10);

    var completedAtUtc =
        startedAtUtc.Add(monthTime);

    var measurement = new MonthlyMeasurement(
        index: 1,
        fromDate: new GameDate(1936, 1, 1),
        toDate: new GameDate(1936, 2, 1),
        elapsedTime: monthTime,
        startedAtUtc: startedAtUtc,
        completedAtUtc: completedAtUtc);

    var score = new BenchmarkScore(
        value: 1000m,
        formulaVersion: "1.0",
        baselineMonthTimeSeconds: 10m,
        description: "Test benchmark score");

    var statistics = new BenchmarkStatistics(
        measuredMonthCount: 1,
        averageMonthTime: monthTime,
        medianMonthTime: monthTime,
        minimumMonthTime: monthTime,
        maximumMonthTime: monthTime,
        standardDeviation: TimeSpan.Zero,
        totalMeasuredTime: monthTime,
        estimatedYearTime: TimeSpan.FromSeconds(120),
        score: score);

    return new BenchmarkResult(
        id: id ?? Guid.NewGuid(),
        name: name,
        startedAtUtc: startedAtUtc,
        completedAtUtc: completedAtUtc,
        measurements: [measurement],
        statistics: statistics);
}

    private static BenchmarkResult CreateUpdatedResult(
        Guid id)
    {
        return CreateResult(
        id: id,
        name: "Updated benchmark");
    }

    private static string CreateTemporaryFilePath()
    {
        return Path.Combine(
            Path.GetTempPath(),
            "HOI4Benchmark.Tests",
            Guid.NewGuid().ToString("N"),
            "results.json");
    }

    private static void DeleteTemporaryDirectory(
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