using HOI4Benchmark.Application.Exceptions;
using HOI4Benchmark.Infrastructure.Parsing;

namespace HOI4Benchmark.Tests.Infrastructure;

public sealed class InitialSaveDateParserTests
{
    [Fact]
    public async Task ParseAsync_ShouldParseQuotedDate()
    {
        var savePath = await CreateSaveAsync(
            """
            HOI4txt
            date="1936.1.1"
            player="GER"
            """);

        try
        {
            var parser = new InitialSaveDateParser();

            var result = await parser.ParseAsync(savePath);

            Assert.Equal(1936, result.Year);
            Assert.Equal(1, result.Month);
            Assert.Equal(1, result.Day);
        }
        finally
        {
            File.Delete(savePath);
        }
    }

    [Fact]
    public async Task ParseAsync_ShouldParseUnquotedDate()
    {
        var savePath = await CreateSaveAsync(
            """
            HOI4txt
            date=1941.6.22
            player="SOV"
            """);

        try
        {
            var parser = new InitialSaveDateParser();

            var result = await parser.ParseAsync(savePath);

            Assert.Equal(1941, result.Year);
            Assert.Equal(6, result.Month);
            Assert.Equal(22, result.Day);
        }
        finally
        {
            File.Delete(savePath);
        }
    }

    [Fact]
    public async Task ParseAsync_ShouldIgnoreNestedDateValues()
    {
        var savePath = await CreateSaveAsync(
            """
            HOI4txt
            previous={
                date="1935.12.31"
            }
            date="1936.1.1"
            """);

        try
        {
            var parser = new InitialSaveDateParser();

            var result = await parser.ParseAsync(savePath);

            Assert.Equal(1936, result.Year);
            Assert.Equal(1, result.Month);
            Assert.Equal(1, result.Day);
        }
        finally
        {
            File.Delete(savePath);
        }
    }

    [Fact]
    public async Task ParseAsync_ShouldThrowWhenDateIsMissing()
    {
        var savePath = await CreateSaveAsync(
            """
            HOI4txt
            player="GER"
            difficulty="normal"
            """);

        try
        {
            var parser = new InitialSaveDateParser();

            await Assert.ThrowsAsync<SaveDateParseException>(
                () => parser.ParseAsync(savePath));
        }
        finally
        {
            File.Delete(savePath);
        }
    }

    [Fact]
    public async Task ParseAsync_ShouldThrowForZipSave()
    {
        var savePath = CreateTemporaryPath();

        await File.WriteAllBytesAsync(
            savePath,
            [0x50, 0x4B, 0x03, 0x04]);

        try
        {
            var parser = new InitialSaveDateParser();

            var exception =
                await Assert.ThrowsAsync<SaveDateParseException>(
                    () => parser.ParseAsync(savePath));

            Assert.Contains(
                "ZIP",
                exception.Message,
                StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            File.Delete(savePath);
        }
    }

    [Fact]
    public async Task ParseAsync_ShouldThrowWhenFileDoesNotExist()
    {
        var parser = new InitialSaveDateParser();

        var missingPath = Path.Combine(
            Path.GetTempPath(),
            $"{Guid.NewGuid():N}.hoi4");

        await Assert.ThrowsAsync<FileNotFoundException>(
            () => parser.ParseAsync(missingPath));
    }

    private static async Task<string> CreateSaveAsync(
        string content)
    {
        var path = CreateTemporaryPath();

        await File.WriteAllTextAsync(
            path,
            content);

        return path;
    }

    private static string CreateTemporaryPath()
    {
        return Path.Combine(
            Path.GetTempPath(),
            $"hoi4-benchmark-{Guid.NewGuid():N}.hoi4");
    }
}