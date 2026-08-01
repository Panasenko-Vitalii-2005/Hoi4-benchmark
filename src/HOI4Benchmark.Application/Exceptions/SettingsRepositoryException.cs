namespace HOI4Benchmark.Application.Exceptions;

public sealed class SettingsRepositoryException : Exception
{
    public SettingsRepositoryException(string message)
        : base(message)
    {
    }

    public SettingsRepositoryException(
        string message,
        Exception innerException)
        : base(message, innerException)
    {
    }
}