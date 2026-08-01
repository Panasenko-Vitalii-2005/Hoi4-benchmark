namespace HOI4Benchmark.Application.Exceptions;

public sealed class ResultRepositoryException : Exception
{
    public ResultRepositoryException(string message)
        : base(message)
    {
    }

    public ResultRepositoryException(
        string message,
        Exception innerException)
        : base(message, innerException)
    {
    }
}