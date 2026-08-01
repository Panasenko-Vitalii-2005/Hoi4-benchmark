using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using HOI4Benchmark.Application.Abstractions;
using HOI4Benchmark.Application.Export;
using HOI4Benchmark.Domain.Benchmarks;

namespace HOI4Benchmark.Infrastructure.Diagnostics;

public sealed class DiagnosticsBundleService
    : IDiagnosticsBundleService
{
    private static readonly JsonSerializerOptions JsonOptions =
        new()
        {
            WriteIndented = true,
            PropertyNamingPolicy =
                JsonNamingPolicy.CamelCase
        };

    private readonly IAppLogger _logger;
    private readonly ISystemInformationProvider
        _systemInformationProvider;

    private readonly ISettingsService _settingsService;
    private readonly IResultService _resultService;
    private readonly IExportService _exportService;

    public DiagnosticsBundleService(
        IAppLogger logger,
        ISystemInformationProvider systemInformationProvider,
        ISettingsService settingsService,
        IResultService resultService,
        IExportService exportService)
    {
        _logger = logger
            ?? throw new ArgumentNullException(
                nameof(logger));

        _systemInformationProvider =
            systemInformationProvider
            ?? throw new ArgumentNullException(
                nameof(systemInformationProvider));

        _settingsService = settingsService
            ?? throw new ArgumentNullException(
                nameof(settingsService));

        _resultService = resultService
            ?? throw new ArgumentNullException(
                nameof(resultService));

        _exportService = exportService
            ?? throw new ArgumentNullException(
                nameof(exportService));
    }

    public async Task CreateBundleAsync(
        string destinationPath,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            destinationPath);

        string fullDestinationPath =
            Path.GetFullPath(destinationPath);

        string? destinationDirectory =
            Path.GetDirectoryName(
                fullDestinationPath);

        if (string.IsNullOrWhiteSpace(
                destinationDirectory))
        {
            throw new ArgumentException(
                "Diagnostics destination must contain a directory.",
                nameof(destinationPath));
        }

        Directory.CreateDirectory(
            destinationDirectory);

        string temporaryDirectory =
            Path.Combine(
                Path.GetTempPath(),
                "HOI4Benchmark",
                "diagnostics",
                Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(
            temporaryDirectory);

        try
        {
            await _logger.LogInformationAsync(
                "Diagnostics bundle creation started.",
                cancellationToken);

            await WriteManifestAsync(
                temporaryDirectory,
                cancellationToken);

            await WriteSystemInformationAsync(
                temporaryDirectory,
                cancellationToken);

            await WriteSettingsAsync(
                temporaryDirectory,
                cancellationToken);

            await WriteResultsAsync(
                temporaryDirectory,
                cancellationToken);

            CopyCurrentLog(
                temporaryDirectory);

            if (File.Exists(fullDestinationPath))
            {
                File.Delete(fullDestinationPath);
            }

            ZipFile.CreateFromDirectory(
                temporaryDirectory,
                fullDestinationPath,
                CompressionLevel.Optimal,
                includeBaseDirectory: false);

            await _logger.LogInformationAsync(
                "Diagnostics bundle created successfully.",
                cancellationToken);
        }
        catch (Exception exception)
        {
            await _logger.LogErrorAsync(
                exception,
                "Diagnostics bundle creation failed.",
                cancellationToken);

            throw;
        }
        finally
        {
            if (Directory.Exists(
                    temporaryDirectory))
            {
                Directory.Delete(
                    temporaryDirectory,
                    recursive: true);
            }
        }
    }

    private static async Task WriteManifestAsync(
        string directory,
        CancellationToken cancellationToken)
    {
        Assembly assembly =
            typeof(DiagnosticsBundleService)
                .Assembly;

        string applicationVersion =
            assembly.GetName().Version?.ToString()
            ?? "unknown";

        string content =
            $"""
            HOI4 Benchmark diagnostics bundle

            Generated at UTC: {DateTimeOffset.UtcNow:O}
            Application version: {applicationVersion}
            .NET version: {Environment.Version}
            Operating system: {Environment.OSVersion}
            Process architecture: {RuntimeInformation.ProcessArchitecture}
            OS architecture: {RuntimeInformation.OSArchitecture}

            Privacy:
            - Windows username is not included.
            - Computer name is not included.
            - Save and game paths are redacted.
            - Benchmark result names are anonymized.
            - Exact result timestamps are excluded.
            - Measurement warnings are excluded.
            """;

        await File.WriteAllTextAsync(
            Path.Combine(
                directory,
                "manifest.txt"),
            content,
            Encoding.UTF8,
            cancellationToken);
    }

    private async Task WriteSystemInformationAsync(
        string directory,
        CancellationToken cancellationToken)
    {
        object systemInformation =
            await _systemInformationProvider
                .GetSystemInformationAsync();

        string json = JsonSerializer.Serialize(
            systemInformation,
            JsonOptions);

        await File.WriteAllTextAsync(
            Path.Combine(
                directory,
                "system-information.json"),
            json,
            Encoding.UTF8,
            cancellationToken);
    }

    private async Task WriteSettingsAsync(
        string directory,
        CancellationToken cancellationToken)
    {
        object settings =
            await _settingsService
                .GetSettingsAsync();

        JsonNode? node =
            JsonSerializer.SerializeToNode(
                settings,
                JsonOptions);

        if (node is JsonObject settingsObject)
        {
            RedactProperty(
                settingsObject,
                "gamePath");

            RedactProperty(
                settingsObject,
                "savePath");
        }

        string json =
            node?.ToJsonString(JsonOptions)
            ?? "{}";

        await File.WriteAllTextAsync(
            Path.Combine(
                directory,
                "settings.json"),
            json,
            Encoding.UTF8,
            cancellationToken);
    }

    private async Task WriteResultsAsync(
        string directory,
        CancellationToken cancellationToken)
    {
        IEnumerable<BenchmarkResult> results =
            await _resultService
                .GetAllResultsAsync();

        var privacyOptions =
            new ExportPrivacyOptions
            {
                AnonymizeResultNames = true,
                ExcludeExactTimestamps = true,
                ExcludeWarnings = true
            };

        string json =
            await _exportService
                .ExportDetailedResultsToJsonAsync(
                    results,
                    privacyOptions);

        await File.WriteAllTextAsync(
            Path.Combine(
                directory,
                "results.json"),
            json,
            Encoding.UTF8,
            cancellationToken);
    }

    private void CopyCurrentLog(
        string directory)
    {
        if (!File.Exists(
                _logger.CurrentLogFilePath))
        {
            return;
        }

        File.Copy(
            _logger.CurrentLogFilePath,
            Path.Combine(
                directory,
                "app.log"),
            overwrite: true);
    }

    private static void RedactProperty(
        JsonObject settingsObject,
        string propertyName)
    {
        if (settingsObject.ContainsKey(
                propertyName))
        {
            settingsObject[propertyName] =
                "<redacted>";
        }
    }
}