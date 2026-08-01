using HOI4Benchmark.Infrastructure.SystemInformation;
using System.Runtime.Versioning;
namespace HOI4Benchmark.Tests.Infrastructure;
[SupportedOSPlatform("windows")]
public sealed class WindowsSystemInformationProviderTests
{
    [Fact]
    public async Task GetSystemInformationAsync_ShouldReturnInformation()
    {
        var provider =
            new WindowsSystemInformationProvider();

        var info =
            await provider.GetSystemInformationAsync();

        Assert.NotNull(info);

        Assert.False(string.IsNullOrWhiteSpace(
            info.OperatingSystem));

        Assert.False(string.IsNullOrWhiteSpace(
            info.ComputerName));

        Assert.True(
            info.LogicalProcessorCount > 0);

        Assert.False(string.IsNullOrWhiteSpace(
            info.DotNetVersion));
    }
}