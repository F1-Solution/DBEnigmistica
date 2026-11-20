using DBEnigmistica.Application.FornitoreAi;
using DBEnigmistica.Application.Services.Schema;
using DBEnigmistica.Domain.AI;
using DBEnigmistica.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace DBEnigmistica.Tests.Unit.Services;

public class SchemaProposalServiceTests
{
    private readonly Mock<IAiGameAnalyzer> _analizzatore = new();
    private readonly SchemaProposalService _servizio;

    public SchemaProposalServiceTests()
    {
        _servizio = new SchemaProposalService(NullLogger<SchemaProposalService>.Instance, _analizzatore.Object);
    }

    private static GameTableSchema NuovoSchema(string tipo = "Sudoku") => new()
    {
        GameTypeName = tipo,
        TableName = "sudoku_games",
        Columns = [new ColumnSchema { Name = "Grid", DataType = "text" }],
    };

    [Fact]
    public async Task GenerateSchemaProposalAsync_InoltraDescrizioneETipoEDaLoSchema()
    {
        var atteso = NuovoSchema();
        _analizzatore
            .Setup(a => a.GenerateTableSchemaAsync("griglia 9x9", "Sudoku", It.IsAny<CancellationToken>()))
            .ReturnsAsync(atteso);

        var schema = await _servizio.GenerateSchemaProposalAsync("Sudoku", "griglia 9x9");

        schema.Should().BeSameAs(atteso);
    }

    [Fact]
    public async Task GenerateSchemaProposalAsync_ErroreDelFornitore_LoRilancia()
    {
        _analizzatore
            .Setup(a => a.GenerateTableSchemaAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Fornitore non disponibile"));

        var azione = () => _servizio.GenerateSchemaProposalAsync("Sudoku", "griglia");

        await azione.Should().ThrowAsync<InvalidOperationException>().WithMessage("*non disponibile*");
    }

    [Fact]
    public async Task GenerateSchemaFromMarkdownContentAsync_InoltraIlTestoEDaLoSchema()
    {
        var atteso = NuovoSchema("Futoshiki");
        _analizzatore
            .Setup(a => a.AnalyzeGameFromMarkdownAsync("# Futoshiki", It.IsAny<CancellationToken>()))
            .ReturnsAsync(atteso);

        var schema = await _servizio.GenerateSchemaFromMarkdownContentAsync("# Futoshiki");

        schema.GameTypeName.Should().Be("Futoshiki");
    }

    [Fact]
    public async Task GenerateSchemaFromMarkdownContentAsync_ErroreDelFornitore_LoRilancia()
    {
        _analizzatore
            .Setup(a => a.AnalyzeGameFromMarkdownAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new TimeoutException());

        var azione = () => _servizio.GenerateSchemaFromMarkdownContentAsync("testo");

        await azione.Should().ThrowAsync<TimeoutException>();
    }

    [Fact]
    public async Task GenerateSchemaFromMarkdownAsync_FileInesistente_LanciaFileNotFound()
    {
        var percorso = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "manca.md");

        var azione = () => _servizio.GenerateSchemaFromMarkdownAsync(percorso);

        await azione.Should().ThrowAsync<FileNotFoundException>();
        _analizzatore.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GenerateSchemaFromMarkdownAsync_FileEsistente_PassaIlContenutoAlFornitore()
    {
        var percorso = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".md");
        await File.WriteAllTextAsync(percorso, "# Nonogramma");
        try
        {
            _analizzatore
                .Setup(a => a.AnalyzeGameFromMarkdownAsync("# Nonogramma", It.IsAny<CancellationToken>()))
                .ReturnsAsync(NuovoSchema("Nonogramma"));

            var schema = await _servizio.GenerateSchemaFromMarkdownAsync(percorso);

            schema.GameTypeName.Should().Be("Nonogramma");
        }
        finally
        {
            File.Delete(percorso);
        }
    }
}
