using HOI4Benchmark.Domain.Game;
using HOI4Benchmark.Domain.Common;

namespace HOI4Benchmark.Application.Abstractions;

public interface IInitialSaveDateParser
{
    Task<GameDate> ParseAsync(
        string savePath,
        CancellationToken cancellationToken = default);
}