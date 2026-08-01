namespace HOI4Benchmark.Application.Abstractions;
using HOI4Benchmark.Application.Models;
public interface ISystemInformationProvider : IApplicationService
{
    Task<SystemInformation> GetSystemInformationAsync();
}