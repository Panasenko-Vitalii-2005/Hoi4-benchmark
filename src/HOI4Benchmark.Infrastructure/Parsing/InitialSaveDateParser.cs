using System.Buffers;
using System.Text;
using System.Text.RegularExpressions;
using HOI4Benchmark.Application.Abstractions;
using HOI4Benchmark.Application.Exceptions;
using HOI4Benchmark.Domain.Game;
using HOI4Benchmark.Domain.Common;

namespace HOI4Benchmark.Infrastructure.Parsing;

public sealed partial class InitialSaveDateParser
    : IInitialSaveDateParser
{
    private const int DefaultReadBufferSize = 64 * 1024;
    private const int MaximumBytesToInspect = 4 * 1024 * 1024;

    private readonly int _readBufferSize;
    private readonly int _maximumBytesToInspect;

    public InitialSaveDateParser()
        : this(
            DefaultReadBufferSize,
            MaximumBytesToInspect)
    {
    }

    public InitialSaveDateParser(
        int readBufferSize,
        int maximumBytesToInspect)
    {
        if (readBufferSize <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(readBufferSize));
        }

        if (maximumBytesToInspect < readBufferSize)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maximumBytesToInspect),
                "Maximum inspected byte count cannot be smaller than the buffer.");
        }

        _readBufferSize = readBufferSize;
        _maximumBytesToInspect = maximumBytesToInspect;
    }

    public async Task<GameDate> ParseAsync(
        string savePath,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(savePath);

        var fullPath = Path.GetFullPath(savePath);

        if (!File.Exists(fullPath))
{
    throw new FileNotFoundException(
        "The selected HOI4 save file was not found.");
}

        try
        {
            await using var stream = new FileStream(
                fullPath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite | FileShare.Delete,
                _readBufferSize,
                FileOptions.Asynchronous |
                FileOptions.SequentialScan);

            if (stream.Length == 0)
            {
                throw new SaveDateParseException(
    "The selected HOI4 save file is empty.");
            }

            var fileFormat = await DetectFormatAsync(
                stream,
                cancellationToken);

            stream.Position = 0;

            if (fileFormat == SaveFileFormat.Zip)
            {
                throw new SaveDateParseException(
                    "The HOI4 save is ZIP-compressed. " +
                    "The initial parser currently supports text saves only. " +
                    "Disable binary saves in HOI4 or add ZIP parsing later.");
            }

            return await ParseTextSaveAsync(
                stream,
                fullPath,
                cancellationToken);
        }
        catch (SaveDateParseException)
        {
            throw;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (IOException exception)
{
    throw new SaveDateParseException(
        "The HOI4 save file could not be opened.",
        exception);
}
        catch (UnauthorizedAccessException exception)
{
    throw new SaveDateParseException(
        "Access to the HOI4 save file was denied.",
        exception);
}
    }

    private static GameDate? TryFindRootDate(string content)
{
    var braceDepth = 0;
    var insideQuotes = false;
    var lineStartDepth = 0;
    var lineStart = 0;

    for (var index = 0; index <= content.Length; index++)
    {
        var reachedEnd = index == content.Length;
        var current = reachedEnd ? '\n' : content[index];

        if (current == '\n' || reachedEnd)
        {
            var lineLength = index - lineStart;

            if (lineStartDepth == 0 && lineLength > 0)
            {
                var line = content
                    .AsSpan(lineStart, lineLength)
                    .Trim();

                var match = RootDateLineRegex().Match(
                    line.ToString());

                if (match.Success)
                {
                    return CreateGameDate(match);
                }
            }

            lineStart = index + 1;
            lineStartDepth = braceDepth;

            continue;
        }

        if (current == '"' &&
            (index == 0 || content[index - 1] != '\\'))
        {
            insideQuotes = !insideQuotes;
            continue;
        }

        if (insideQuotes)
        {
            continue;
        }

        if (current == '{')
        {
            braceDepth++;
        }
        else if (current == '}')
        {
            braceDepth = Math.Max(0, braceDepth - 1);
        }
    }

    return null;
}

    private async Task<GameDate> ParseTextSaveAsync(
        Stream stream,
        string savePath,
        CancellationToken cancellationToken)
    {
        var buffer = ArrayPool<byte>.Shared.Rent(
            _readBufferSize);

        try
        {
            var collectedText = new StringBuilder();
            var totalBytesRead = 0;

            while (totalBytesRead < _maximumBytesToInspect)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var bytesRemaining =
                    _maximumBytesToInspect - totalBytesRead;

                var bytesToRead = Math.Min(
                    buffer.Length,
                    bytesRemaining);

                var bytesRead = await stream.ReadAsync(
                    buffer.AsMemory(0, bytesToRead),
                    cancellationToken);

                if (bytesRead == 0)
                {
                    break;
                }

                totalBytesRead += bytesRead;

                collectedText.Append(
                    Encoding.UTF8.GetString(
                        buffer,
                        0,
                        bytesRead));

                GameDate? gameDate = TryFindRootDate(
    collectedText.ToString());

                if (gameDate.HasValue)
                {
                    return gameDate.Value;
                }

                TrimSearchBuffer(collectedText);
            }

            throw new SaveDateParseException(
                $"The initial game date was not found in the first " +
                $"{_maximumBytesToInspect} bytes of save file: {savePath}");
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    private static GameDate CreateGameDate(
        Match match)
    {
        if (!int.TryParse(
                match.Groups["year"].Value,
                out var year) ||
            !int.TryParse(
                match.Groups["month"].Value,
                out var month) ||
            !int.TryParse(
                match.Groups["day"].Value,
                out var day))
        {
            throw new SaveDateParseException(
                $"Invalid HOI4 date value: {match.Value}");
        }

        try
        {
            return new GameDate(year, month, day);
        }
        catch (DomainException exception)
        {
            throw new SaveDateParseException(
                $"HOI4 save contains an invalid date: " +
                $"{year}.{month}.{day}",
                exception);
        }
        catch (ArgumentException exception)
        {
            throw new SaveDateParseException(
                $"HOI4 save contains an invalid date: " +
                $"{year}.{month}.{day}",
                exception);
        }
    }

    private static async Task<SaveFileFormat> DetectFormatAsync(
        Stream stream,
        CancellationToken cancellationToken)
    {
        var signature = new byte[4];

        var bytesRead = await stream.ReadAsync(
            signature.AsMemory(),
            cancellationToken);

        if (bytesRead >= 2 &&
            signature[0] == 0x50 &&
            signature[1] == 0x4B)
        {
            return SaveFileFormat.Zip;
        }

        return SaveFileFormat.Text;
    }

    private static void TrimSearchBuffer(
        StringBuilder builder)
    {
        const int retainedCharacters = 2048;

        if (builder.Length <= retainedCharacters)
        {
            return;
        }

        builder.Remove(
            0,
            builder.Length - retainedCharacters);
    }

    [GeneratedRegex(
    @"^date\s*=\s*""?(?<year>\d{1,6})\.(?<month>\d{1,2})\.(?<day>\d{1,2})(?:\.(?<hour>\d{1,2}))?""?\s*$",
    RegexOptions.CultureInvariant)]
private static partial Regex RootDateLineRegex();

    private enum SaveFileFormat
    {
        Text,
        Zip
    }
}