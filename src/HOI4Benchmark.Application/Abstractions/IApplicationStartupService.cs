using System.Threading.Tasks;

namespace HOI4Benchmark.Application.Abstractions
{
    /// <summary>
    /// Service for application startup and initialization
    /// </summary>
    public interface IApplicationStartupService : IApplicationService
    {
        /// <summary>
        /// Initializes the application
        /// </summary>
        Task InitializeAsync();

        /// <summary>
        /// Ensures application is properly configured
        /// </summary>
        Task EnsureConfigurationAsync();
    }
}