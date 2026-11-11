using DBEnigmistica.Domain.Diritti;
using System.Globalization;
using DBEnigmistica.Application.Services.Export;
using DBEnigmistica.Domain.Entities;
using DBEnigmistica.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace DBEnigmistica.Tests.Unit.Services;

public class CsvExportTests
{
    [Theory]
    [InlineData("=SUM(A1:A2)", "'=SUM(A1:A2)")]
    [InlineData("+1+1", "'+1+1")]
    [InlineData("-2+3", "'-2+3")]
    [InlineData("@cmd", "'@cmd")]
    [InlineData("\tdato", "'\tdato")]
    [InlineData("\rdato", "'\rdato")]
    [InlineData(" =HYPERLINK(\"http://x\")", "' =HYPERLINK(\"http://x\")")]
    [InlineData("   +1", "'   +1")]
    [InlineData("   ", "   ")]
    [InlineData("testo normale", "testo normale")]
    [InlineData("a=b", "a=b")]
    [InlineData("x;=1+1", "x;=1+1")]
    [InlineData("", "")]
    [InlineData(null, "")]
    public void Neutralize_PrefixesFormulaTriggers(string? input, string expected)
    {
        CsvFieldFormatter.Neutralize(input).Should().Be(expected);
    }

    [Fact]
    public void NeutralizeAndDoubleQuotes_NeutralizesBeforeDoublingQuotes()
    {
        CsvFieldFormatter.NeutralizeAndDoubleQuotes("=HYPERLINK(\"http://x\")")
            .Should().Be("'=HYPERLINK(\"\"http://x\"\")");
    }

    [Theory]
    [InlineData("uno, due", "\"uno, due\"")]
    [InlineData("Cruciverba;=1+1;x", "\"Cruciverba;=1+1;x\"")]
    [InlineData("x;=1+1", "\"x;=1+1\"")]
    [InlineData(" =HYPERLINK(\"http://x\")", "\"' =HYPERLINK(\"\"http://x\"\")\"")]
    [InlineData("=1,2", "\"'=1,2\"")]
    [InlineData("riga1\nriga2", "\"riga1\nriga2\"")]
    [InlineData("semplice", "\"semplice\"")]
    [InlineData("", "\"\"")]
    [InlineData(null, "\"\"")]
    public void Format_AlwaysQuotesTextFields(string? input, string expected)
    {
        CsvFieldFormatter.Format(input).Should().Be(expected);
    }

    [Fact]
    public async Task ExportGamesAsCsv_StartsWithSeparatorDirective()
    {
        var csv = await ExportAsync(new Game { Origine = Origine.Rivista, Id = 1, Title = "t" });

        csv.Split(Environment.NewLine)[0].Should().Be("sep=,");
        csv.Split(Environment.NewLine)[1].Should().StartWith("Id,GameType,Title,");
    }

    [Fact]
    public async Task ExportGamesAsCsv_NeutralizesFormulaInTitle()
    {
        var csv = await ExportAsync(new Game { Origine = Origine.Rivista, Id = 1, Title = "=1+1", Description = "ok" });

        csv.Should().Contain("\"'=1+1\"");
        csv.Should().NotContain(",=1+1");
    }

    [Fact]
    public async Task ExportGamesAsCsv_QuotesSemicolonInjection()
    {
        var csv = await ExportAsync(new Game { Origine = Origine.Rivista, Id = 1, Title = "Cruciverba;=1+1;x" });

        csv.Should().Contain(",\"Cruciverba;=1+1;x\",");
    }

    [Fact]
    public async Task ExportGamesAsCsv_NeutralizesFormulaAfterLeadingSpace()
    {
        var csv = await ExportAsync(new Game { Origine = Origine.Rivista, Id = 1, Title = " =HYPERLINK(\"http://x\")" });

        csv.Should().Contain("\"' =HYPERLINK(\"\"http://x\"\")\"");
    }

    [Fact]
    public async Task ExportGamesAsCsv_KeepsEmbeddedNewlineInsideQuotes()
    {
        var csv = await ExportAsync(new Game { Origine = Origine.Rivista, Id = 1, Title = "riga1\nriga2" });

        csv.Should().Contain("\"riga1\nriga2\"");
    }

    [Fact]
    public async Task ExportGamesAsCsv_WritesNegativeNumbersUnaltered()
    {
        var csv = await ExportAsync(new Game { Origine = Origine.Rivista, Id = 1, Title = "t", PositionX = -5, PositionY = -7 });

        csv.Should().Contain(",-5,-7,");
        csv.Should().NotContain("'-5");
    }

    [Fact]
    public async Task ExportGamesAsCsv_WithItalianCulture_WritesInvariantNumbersAndKeepsColumnCount()
    {
        var previous = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = new CultureInfo("it-IT");
        try
        {
            var csv = await ExportAsync(new Game
            {
                Origine = Origine.Rivista,
                Id = 1,
                Title = "t",
                ClassificationConfidence = 0.95,
                ExtractedDate = new DateTime(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc)
            });

            var lines = csv.Split(Environment.NewLine);
            lines[2].Should().Contain(",0.95,");
            lines[2].Should().NotContain("0,95");
            lines[2].Should().Contain("2026-01-02T03:04:05.0000000Z");
            lines[2].Split(',').Length.Should().Be(lines[1].Split(',').Length);
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }

    private static async Task<string> ExportAsync(Game game)
    {
        var gameRepository = new Mock<IGameRepository>();
        gameRepository
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Game> { game });

        var service = new DataExportService(
            Mock.Of<IMagazineRepository>(),
            gameRepository.Object,
            Mock.Of<ILogger<DataExportService>>());

        return await service.ExportGamesAsCsvAsync();
    }
}
