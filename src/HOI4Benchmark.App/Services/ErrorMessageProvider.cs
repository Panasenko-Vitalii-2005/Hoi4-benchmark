using HOI4Benchmark.Application.Exceptions;
using HOI4Benchmark.Domain.Common;
using System.IO;

namespace HOI4Benchmark.App.Services;

public static class ErrorMessageProvider
{
    public static string GetMessage(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        Exception actualException =
            UnwrapException(exception);

        return actualException switch
        {
            DirectoryNotFoundException =>
                "The configured folder was not found. " +
                "Check the path in Settings.",

            FileNotFoundException =>
                "The required file was not found. " +
                "Check the configured path and try again.",

            UnauthorizedAccessException =>
                "Access was denied. Check the selected folder permissions.",

            SaveDateParseException =>
                "The HOI4 save file could not be read. " +
                "Make sure it is a valid text-format save file.",

            IOException =>
                "The file could not be accessed. " +
                "It may be in use by another application.",

            OperationCanceledException =>
                "The operation was cancelled.",

            ArgumentException =>
                "One or more entered values are invalid.",

            DomainException domainException =>
                GetDomainMessage(domainException),

            InvalidOperationException =>
                "The operation cannot be completed in the current application state.",

            _ =>
                "An unexpected error occurred. " +
                "Technical details will be available in diagnostics."
        };
    }

    public static string GetMessage(
        string action,
        Exception exception)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(action);
        ArgumentNullException.ThrowIfNull(exception);

        return $"{action}: {GetMessage(exception)}";
    }

    private static Exception UnwrapException(
        Exception exception)
    {
        Exception current = exception;

        while (current is AggregateException aggregateException &&
               aggregateException.InnerExceptions.Count == 1)
        {
            current = aggregateException.InnerExceptions[0];
        }

        return current;
    }

    private static string GetDomainMessage(
        DomainException exception)
    {
        string message = exception.Message;

        if (message.Contains(
                "already active",
                StringComparison.OrdinalIgnoreCase))
        {
            return "Another benchmark is already active. " +
                   "Stop or cancel it before starting a new one.";
        }

        if (message.Contains(
                "already finished",
                StringComparison.OrdinalIgnoreCase))
        {
            return "This benchmark session has already finished.";
        }

        if (message.Contains(
                "not found",
                StringComparison.OrdinalIgnoreCase))
        {
            return "The requested benchmark session was not found.";
        }

        if (message.Contains(
                "name is required",
                StringComparison.OrdinalIgnoreCase))
        {
            return "Enter a name for the benchmark.";
        }

        return message;
    }
}