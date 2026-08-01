using HOI4Benchmark.Domain.Common;
using HOI4Benchmark.Domain.Game;

namespace HOI4Benchmark.Tests.Domain;

public sealed class GameDateTests
{
    [Theory]
    [InlineData(0, 1, 1)]
    [InlineData(1936, 0, 1)]
    [InlineData(1936, 13, 1)]
    [InlineData(1936, 2, 30)]
    public void Constructor_WhenDateIsInvalid_ThrowsDomainException(int year, int month, int day)
    {
        Assert.Throws<DomainException>(() => new GameDate(year, month, day));
    }

    [Fact]
    public void CompareTo_WhenDatesAreDifferent_OrdersByYearMonthAndDay()
    {
        GameDate january = new(1936, 1, 1);
        GameDate february = new(1936, 2, 1);
        GameDate nextYear = new(1937, 1, 1);

        Assert.True(january < february);
        Assert.True(nextYear > february);
        Assert.True(february.IsAfter(january));
        Assert.True(january.IsBefore(nextYear));
    }

    [Fact]
    public void MonthsUntil_WhenDatesCrossYearBoundary_ReturnsSignedMonthDifference()
    {
        GameDate november = new(1936, 11, 1);
        GameDate february = new(1937, 2, 1);

        Assert.Equal(3, november.MonthsUntil(february));
        Assert.Equal(-3, february.MonthsUntil(november));
    }

    [Theory]
    [InlineData("1936.1.1", 1936, 1, 1)]
    [InlineData("1936-02-01", 1936, 2, 1)]
    [InlineData("1936/12/31", 1936, 12, 31)]
    public void TryParse_WhenValueIsSupported_ReturnsGameDate(string value, int year, int month, int day)
    {
        bool parsed = GameDate.TryParse(value, out GameDate result);

        Assert.True(parsed);
        Assert.Equal(new GameDate(year, month, day), result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("1936")]
    [InlineData("1936.13.1")]
    [InlineData("not-a-date")]
    public void TryParse_WhenValueIsInvalid_ReturnsFalse(string? value)
    {
        bool parsed = GameDate.TryParse(value, out _);

        Assert.False(parsed);
    }

    [Fact]
    public void ToString_ReturnsStableInvariantFormat()
    {
        GameDate date = new(1936, 1, 2);

        Assert.Equal("1936.01.02", date.ToString());
    }
}
