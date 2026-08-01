namespace HOI4Benchmark.Application.Models;

public sealed record SystemInformation(
    string OperatingSystem,
    string OperatingSystemVersion,
    string ComputerName,
    string ProcessorName,
    int LogicalProcessorCount,
    ulong InstalledMemoryMb,
    string GraphicsAdapter,
    string DotNetVersion,
    string Architecture);