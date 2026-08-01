using System.Threading.Tasks;

namespace HOI4Benchmark.Application.Abstractions
{
    /// <summary>
    /// Service for managing application lifecycle operations
    /// </summary>
    public interface IApplicationLifecycleService : IApplicationService
    {
        /// <summary>
        /// Starts the application
        /// </summary>
        Task StartAsync();

        /// <summary>
        /// Stops the application
        /// </summary>
        Task StopAsync();
    }
}