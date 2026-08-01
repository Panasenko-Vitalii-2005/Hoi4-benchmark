namespace HOI4Benchmark.Application.Exceptions;

public sealed class SaveDateParseException : Exception
{
    public SaveDateParseException(string message)
        : base(message)
    {
    }

    public SaveDateParseException(
        string message,
        Exception innerException)
        : base(message, innerException)
    {
    }
}