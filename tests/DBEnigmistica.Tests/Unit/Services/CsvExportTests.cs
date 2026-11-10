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
    [InlineData("testo normale", "testo normale")]
    [InlineData("a=b", "a=b")]
    [InlineData("", "")]
    [InlineData(null, "")]
    public void Neutralize_PrefixesFormulaTriggers(string? input, string expected)
    {
        CsvFieldFormatter.Neutralize(input).Should().Be(expected);
    }

    [Fact]
    public void EscapeQuotes_NeutralizesBeforeQuoting()
    {
        CsvFieldFormatter.EscapeQuotes("=HYPERLINK(\"http://x\")")
            .Should().Be("'=HYPERLINK(\"\"http://x\"\")");
    }

    [Fact]
    public void Format_QuotesFieldsWithCommas()
    {
        CsvFieldFormatter.Format("uno, due").Should().Be("\"uno, due\"");
    }

    [Fact]
    public async Task ExportGamesAsCsv_NeutralizesFormulaInTitle()
    {
        var gameRepository = new Mock<IGameRepository>();
        gameRepository
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Game> { new() { Id = 1, Title = "=1+1", Description = "ok" } });

        var service = new DataExportService(
            Mock.Of<IMagazineRepository>(),
            gameRepository.Object,
            Mock.Of<ILogger<DataExportService>>());

        var csv = await service.ExportGamesAsCsvAsync();

        csv.Should().Contain("'=1+1");
        csv.Should().NotContain(",=1+1");
    }
}
