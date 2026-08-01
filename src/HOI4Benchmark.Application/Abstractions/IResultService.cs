using System.Collections.Generic;
using System.Threading.Tasks;
using HOI4Benchmark.Domain.Benchmarks;

namespace HOI4Benchmark.Application.Abstractions
{
    /// <summary>
    /// Service for managing benchmark results
    /// </summary>
    public interface IResultService : IApplicationService
    {
        /// <summary>
        /// Saves a benchmark result
        /// </summary>
        Task<BenchmarkResult> SaveResultAsync(BenchmarkResult result);

        /// <summary>
        /// Gets all benchmark results
        /// </summary>
        Task<IEnumerable<BenchmarkResult>> GetAllResultsAsync();

        /// <summary>
        /// Gets a specific benchmark result by ID
        /// </summary>
        Task<BenchmarkResult?> GetResultByIdAsync(string id);

        /// <summary>
        /// Deletes a benchmark result
        /// </summary>
        Task<bool> DeleteResultAsync(string id);

        /// <summary>
        /// Gets results within a date range
        /// </summary>
        Task<IEnumerable<BenchmarkResult>> GetResultsByDateRangeAsync(
            System.DateTime startDate, 
            System.DateTime endDate);
    }
}