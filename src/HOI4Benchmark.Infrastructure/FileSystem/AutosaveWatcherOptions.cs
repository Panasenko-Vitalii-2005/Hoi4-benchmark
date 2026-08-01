namespace HOI4Benchmark.Infrastructure.FileSystem;

public sealed class AutosaveWatcherOptions
{
    /// <summary>
    /// How long to wait after the latest file-system event.
    /// </summary>
    public TimeSpan DebounceDelay { get; init; } =
        TimeSpan.FromMilliseconds(750);

    /// <summary>
    /// Delay between file stability checks.
    /// </summary>
    public TimeSpan StabilityCheckInterval { get; init; } =
        TimeSpan.FromMilliseconds(250);

    /// <summary>
    /// Number of consecutive unchanged checks required.
    /// </summary>
    public int RequiredStableChecks { get; init; } = 3;

    /// <summary>
    /// Maximum number of checks during one stability attempt.
    /// </summary>
    public int MaxStabilityChecks { get; init; } = 20;

    /// <summary>
    /// Maximum processing attempts when the file is temporarily unavailable.
    /// </summary>
    public int MaxRetryAttempts { get; init; } = 5;

    /// <summary>
    /// Delay between retry attempts.
    /// </summary>
    public TimeSpan RetryDelay { get; init; } =
        TimeSpan.FromMilliseconds(500);

    public void Validate()
    {
        if (DebounceDelay < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(DebounceDelay));
        }

        if (StabilityCheckInterval <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(StabilityCheckInterval));
        }

        if (RequiredStableChecks <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(RequiredStableChecks));
        }

        if (MaxStabilityChecks < RequiredStableChecks)
        {
            throw new ArgumentOutOfRangeException(
                nameof(MaxStabilityChecks),
                "Max stability checks cannot be lower than required stable checks.");
        }

        if (MaxRetryAttempts <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(MaxRetryAttempts));
        }

        if (RetryDelay < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(RetryDelay));
        }
    }
}