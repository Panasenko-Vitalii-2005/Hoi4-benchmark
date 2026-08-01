using HOI4Benchmark.Application.Abstractions;
using HOI4Benchmark.Domain.Benchmarks;

namespace HOI4Benchmark.Application.Implementations;

public class ResultService : IResultService
{
    private readonly IRepository<BenchmarkResult> _resultRepository;

    public ResultService(
        IRepository<BenchmarkResult> resultRepository)
    {
        _resultRepository = resultRepository 
            ?? throw new ArgumentNullException(nameof(resultRepository));
    }


    public async Task<BenchmarkResult> SaveResultAsync(BenchmarkResult result)
    {
        if (result == null)
        {
            throw new ArgumentNullException(nameof(result));
        }

        await _resultRepository.AddAsync(result);

        return result;
    }


    public async Task<IEnumerable<BenchmarkResult>> GetAllResultsAsync()
    {
        return await _resultRepository.GetAllAsync();
    }


    public async Task<BenchmarkResult?> GetResultByIdAsync(string id)
    {
        if (!Guid.TryParse(id, out var resultId))
        {
            return null;
        }

        return await _resultRepository.GetByIdAsync(resultId);
    }


    public async Task<bool> DeleteResultAsync(string id)
    {
        var result = await GetResultByIdAsync(id);

        if (result == null)
        {
            return false;
        }

        await _resultRepository.DeleteAsync(result);

        return true;
    }


    public async Task<IEnumerable<BenchmarkResult>> GetResultsByDateRangeAsync(
        DateTime startDate,
        DateTime endDate)
    {
        var results = await _resultRepository.GetAllAsync();

        return results.Where(x =>
            x.CompletedAtUtc.Date >= startDate.Date &&
            x.CompletedAtUtc.Date <= endDate.Date);
    }
}