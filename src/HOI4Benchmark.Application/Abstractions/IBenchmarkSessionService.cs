using System.Threading.Tasks;
using HOI4Benchmark.Domain.Benchmarks;

namespace HOI4Benchmark.Application.Abstractions
{
    /// <summary>
    /// Service for managing benchmark sessions
    /// </summary>
    public interface IBenchmarkSessionService : IApplicationService
    {
        /// <summary>
        /// Starts a new benchmark session
        /// </summary>
        Task<BenchmarkSession> StartBenchmarkAsync(string name, int targetMeasuredMonths, int warmupMonths = 0);
        /// <summary>
        /// Stops the current benchmark session
        /// </summary>
        Task<BenchmarkSession> StopBenchmarkAsync(System.Guid sessionId);

        /// <summary>
        /// Gets the current active benchmark session
        /// </summary>
        Task<BenchmarkSession?> GetCurrentSessionAsync();
    }
}