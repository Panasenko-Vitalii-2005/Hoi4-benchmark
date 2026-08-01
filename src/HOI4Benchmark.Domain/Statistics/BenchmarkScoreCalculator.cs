using HOI4Benchmark.Domain.Benchmarks;
using HOI4Benchmark.Domain.Common;

namespace HOI4Benchmark.Domain.Statistics;

public sealed class BenchmarkScoreCalculator
{
    public const string FormulaVersion = "v1";
    public const decimal BaselineMonthTimeSeconds = 10.0m;

    public BenchmarkScore Calculate(TimeSpan averageMonthTime)
    {
        if (averageMonthTime <= TimeSpan.Zero)
        {
            throw new DomainException("Average month time must be greater than zero to calculate benchmark score.");
        }

        decimal averageSeconds = Convert.ToDecimal(averageMonthTime.TotalSeconds);
        decimal score = Math.Round(BaselineMonthTimeSeconds / averageSeconds * 1000m, 2, MidpointRounding.AwayFromZero);

        return new BenchmarkScore(
            score,
            FormulaVersion,
            BaselineMonthTimeSeconds,
            "Score v1: 10 seconds/month baseline equals 1000 points. Faster simulation produces a higher score.");
    }
}
