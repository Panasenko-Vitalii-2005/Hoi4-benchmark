using System.Management;
using System.Runtime.InteropServices;
using HOI4Benchmark.Application.Abstractions;
using SystemInfoModel = HOI4Benchmark.Application.Models.SystemInformation;
using System.Runtime.Versioning;
namespace HOI4Benchmark.Infrastructure.SystemInformation;
[SupportedOSPlatform("windows")]
public sealed class WindowsSystemInformationProvider
    : ISystemInformationProvider
{
    public Task<SystemInfoModel> GetSystemInformationAsync()
    {
        var information = new SystemInfoModel(
            OperatingSystem: RuntimeInformation.OSDescription,
            OperatingSystemVersion: Environment.OSVersion.VersionString,
            ComputerName: Environment.MachineName,
            ProcessorName: GetProcessorName(),
            LogicalProcessorCount: Environment.ProcessorCount,
            InstalledMemoryMb: GetInstalledMemoryMb(),
            GraphicsAdapter: GetGraphicsAdapter(),
            DotNetVersion: RuntimeInformation.FrameworkDescription,
            Architecture: RuntimeInformation.OSArchitecture.ToString());

        return Task.FromResult(information);
    }

    private static string GetProcessorName()
    {
        try
        {
            using var searcher =
                new ManagementObjectSearcher(
                    "SELECT Name FROM Win32_Processor");

            foreach (ManagementObject cpu in searcher.Get())
            {
                return cpu["Name"]?.ToString()?.Trim()
                    ?? "Unknown";
            }
        }
        catch
        {
        }

        return "Unknown";
    }

    private static string GetGraphicsAdapter()
    {
        try
        {
            using var searcher =
                new ManagementObjectSearcher(
                    "SELECT Name FROM Win32_VideoController");

            foreach (ManagementObject gpu in searcher.Get())
            {
                return gpu["Name"]?.ToString()?.Trim()
                    ?? "Unknown";
            }
        }
        catch
        {
        }

        return "Unknown";
    }

    private static ulong GetInstalledMemoryMb()
    {
        try
        {
            using var searcher =
                new ManagementObjectSearcher(
                    "SELECT TotalPhysicalMemory FROM Win32_ComputerSystem");

            foreach (ManagementObject system in searcher.Get())
            {
                if (system["TotalPhysicalMemory"] is ulong bytes)
                {
                    return bytes / 1024 / 1024;
                }
            }
        }
        catch
        {
        }

        return 0;
    }
}