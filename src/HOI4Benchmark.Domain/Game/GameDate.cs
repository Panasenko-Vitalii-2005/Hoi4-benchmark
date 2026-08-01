using System.Globalization;
using HOI4Benchmark.Domain.Common;
using System.Text.Json.Serialization;

namespace HOI4Benchmark.Domain.Game;

public readonly record struct GameDate : IComparable<GameDate>
{
    [JsonConstructor]
    public GameDate(int year, int month, int day)
    {
        if (year < 1)
        {
            throw new DomainException("Game date year must be greater than zero.");
        }

        if (month is < 1 or > 12)
        {
            throw new DomainException("Game date month must be between 1 and 12.");
        }

        int daysInMonth = DateTime.DaysInMonth(year, month);
        if (day < 1 || day > daysInMonth)
        {
            throw new DomainException($"Game date day must be between 1 and {daysInMonth} for {year:D4}.{month:D2}.");
        }

        Year = year;
        Month = month;
        Day = day;
    }

    public int Year { get; }

    public int Month { get; }

    public int Day { get; }

    public int CompareTo(GameDate other)
    {
        int yearComparison = Year.CompareTo(other.Year);
        if (yearComparison != 0)
        {
            return yearComparison;
        }

        int monthComparison = Month.CompareTo(other.Month);
        return monthComparison != 0 ? monthComparison : Day.CompareTo(other.Day);
    }

    public int MonthsUntil(GameDate other) => ((other.Year - Year) * 12) + other.Month - Month;

    public bool IsAfter(GameDate other) => CompareTo(other) > 0;

    public bool IsBefore(GameDate other) => CompareTo(other) < 0;

    public bool IsNextMonthAfter(GameDate previous) => previous.MonthsUntil(this) == 1;

    public GameDate AddMonths(int months)
    {
        DateTime date = new(Year, Month, Day);
        DateTime result = date.AddMonths(months);
        return new GameDate(result.Year, result.Month, result.Day);
    }

    public static bool operator <(GameDate left, GameDate right) => left.CompareTo(right) < 0;

    public static bool operator <=(GameDate left, GameDate right) => left.CompareTo(right) <= 0;

    public static bool operator >(GameDate left, GameDate right) => left.CompareTo(right) > 0;

    public static bool operator >=(GameDate left, GameDate right) => left.CompareTo(right) >= 0;

    public static GameDate Parse(string value)
    {
        return TryParse(value, out GameDate result)
            ? result
            : throw new DomainException($"Invalid game date format: '{value}'. Expected format is yyyy.M.d, yyyy-MM-dd, or yyyy/MM/dd.");
    }

    public static bool TryParse(string? value, out GameDate result)
    {
        result = default;

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        string[] parts = value.Trim().Split(['.', '-', '/'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length != 3)
        {
            return false;
        }

        if (!int.TryParse(parts[0], NumberStyles.None, CultureInfo.InvariantCulture, out int year)
            || !int.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out int month)
            || !int.TryParse(parts[2], NumberStyles.None, CultureInfo.InvariantCulture, out int day))
        {
            return false;
        }

        try
        {
            result = new GameDate(year, month, day);
            return true;
        }
        catch (DomainException)
        {
            return false;
        }
    }

    public override string ToString() => FormattableString.Invariant($"{Year:D4}.{Month:D2}.{Day:D2}");
}
