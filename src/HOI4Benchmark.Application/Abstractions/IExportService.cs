using System.Collections.Generic;
using System.Threading.Tasks;
using HOI4Benchmark.Domain.Benchmarks;
using HOI4Benchmark.Application.Export;

namespace HOI4Benchmark.Application.Abstractions
{
    /// <summary>
    /// Service for exporting benchmark data
    /// </summary>
    public interface IExportService : IApplicationService
    {
        /// <summary>
        /// Exports benchmark results to JSON format
        /// </summary>
        Task<string> ExportToJsonAsync(IEnumerable<BenchmarkResult> results);

        /// <summary>
        /// Exports benchmark results to CSV format
        /// </summary>
        Task<string> ExportToCsvAsync(IEnumerable<BenchmarkResult> results);

        /// <summary>
        /// Exports a single benchmark result to JSON format
        /// </summary>
        Task<string> ExportResultToJsonAsync(BenchmarkResult result);

        /// <summary>
        /// Exports a single benchmark result to CSV format
        /// </summary>
        Task<string> ExportResultToCsvAsync(BenchmarkResult result);

        Task<string> ExportDetailedResultToJsonAsync(
    BenchmarkResult result,
    ExportPrivacyOptions privacyOptions);

Task<string> ExportDetailedResultsToJsonAsync(
    IEnumerable<BenchmarkResult> results,
    ExportPrivacyOptions privacyOptions);

Task<string> ExportMeasurementsToCsvAsync(
    BenchmarkResult result,
    ExportPrivacyOptions privacyOptions);

Task<string> ExportMeasurementsToCsvAsync(
    IEnumerable<BenchmarkResult> results,
    ExportPrivacyOptions privacyOptions);
    }
}