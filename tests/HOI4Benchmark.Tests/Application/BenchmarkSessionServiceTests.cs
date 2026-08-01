using HOI4Benchmark.Application.Implementations;
using HOI4Benchmark.Tests.Fakes;
using HOI4Benchmark.Domain.Benchmarks;
namespace HOI4Benchmark.Tests.Application;

public class BenchmarkSessionServiceTests
{
    [Fact]
    public async Task StartBenchmarkAsync_ShouldCreateSession()
    {
        var repository = new FakeRepository<BenchmarkSession>();

        var service = new BenchmarkSessionService(repository);

        var result = await service.StartBenchmarkAsync(
            "Test benchmark",
            120);

        Assert.NotNull(result);
        Assert.Equal(
            "Test benchmark",
            result.Name);
    }
}