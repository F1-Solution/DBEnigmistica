using DBEnigmistica.Application.Services.Export;
using DBEnigmistica.Domain.Diritti;
using DBEnigmistica.Domain.Entities;
using DBEnigmistica.Domain.Enums;
using DBEnigmistica.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System.Text.Json;

namespace DBEnigmistica.Tests.Unit.Services;

public class DataExportServiceTests
{
    private readonly Mock<IMagazineRepository> _riviste = new();
    private readonly Mock<IGameRepository> _giochi = new();
    private readonly DataExportService _servizio;

    public DataExportServiceTests()
    {
        _servizio = new DataExportService(_riviste.Object, _giochi.Object, NullLogger<DataExportService>.Instance);
    }

    [Fact]
    public async Task ExportGamesAsJsonAsync_ElencoVuoto_ProduceZeroGiochi()
    {
        _giochi.Setup(g => g.GetAllAsync()).ReturnsAsync([]);

        var json = await _servizio.ExportGamesAsJsonAsync();

        using var documento = JsonDocument.Parse(json);
        documento.RootElement.GetProperty("totalGames").GetInt32().Should().Be(0);
        documento.RootElement.GetProperty("games").GetArrayLength().Should().Be(0);
        documento.RootElement.GetProperty("magazineId").ValueKind.Should().Be(JsonValueKind.Null);
    }

    [Fact]
    public async Task ExportGamesAsJsonAsync_ConRivista_InterrogaSoloQuellaRivista()
    {
        _giochi.Setup(g => g.GetByMagazineIdAsync(7)).ReturnsAsync([]);

        var json = await _servizio.ExportGamesAsJsonAsync(magazineId: 7);

        using var documento = JsonDocument.Parse(json);
        documento.RootElement.GetProperty("magazineId").GetInt32().Should().Be(7);
        _giochi.Verify(g => g.GetByMagazineIdAsync(7), Times.Once);
        _giochi.Verify(g => g.GetAllAsync(), Times.Never);
    }

    [Fact]
    public async Task ExportGamesAsJsonAsync_SenzaFormattazione_NonVaAcapo()
    {
        _giochi.Setup(g => g.GetAllAsync()).ReturnsAsync([]);

        var json = await _servizio.ExportGamesAsJsonAsync(prettyPrint: false);

        json.Should().NotContain("\n");
    }

    [Fact]
    public async Task ExportGamesAsCsvAsync_ElencoVuoto_HaSoloDirettivaEIntestazione()
    {
        _giochi.Setup(g => g.GetAllAsync()).ReturnsAsync([]);

        var csv = await _servizio.ExportGamesAsCsvAsync();

        var righe = csv.Split(["\r\n", "\n"], StringSplitOptions.RemoveEmptyEntries);
        righe.Should().HaveCount(2);
        righe[0].Should().Be(CsvFieldFormatter.SeparatorDirective);
        righe[1].Should().StartWith("Id,GameType,Title");
    }

    [Fact]
    public async Task ExportGamesAsCsvAsync_ConRivista_InterrogaSoloQuellaRivista()
    {
        _giochi.Setup(g => g.GetByMagazineIdAsync(3)).ReturnsAsync([]);

        await _servizio.ExportGamesAsCsvAsync(magazineId: 3);

        _giochi.Verify(g => g.GetByMagazineIdAsync(3), Times.Once);
        _giochi.Verify(g => g.GetAllAsync(), Times.Never);
    }

    [Fact]
    public async Task ExportMagazineStatsAsJsonAsync_NessunaRivista_ProduceZeroRiviste()
    {
        _riviste.Setup(m => m.GetAllAsync()).ReturnsAsync([]);

        var json = await _servizio.ExportMagazineStatsAsJsonAsync();

        using var documento = JsonDocument.Parse(json);
        documento.RootElement.GetProperty("totalMagazines").GetInt32().Should().Be(0);
    }

    [Fact]
    public async Task ExportMagazineStatsAsJsonAsync_RivistaInesistente_VieneSaltata()
    {
        var rivista = new Magazine { Id = 5, Title = "Rivista", SourceFilePath = "r.pdf" };
        _riviste.Setup(m => m.GetAllAsync()).ReturnsAsync([rivista]);
        _riviste.Setup(m => m.GetWithPagesAndGamesAsync(5)).ReturnsAsync((Magazine?)null);

        var json = await _servizio.ExportMagazineStatsAsJsonAsync();

        using var documento = JsonDocument.Parse(json);
        documento.RootElement.GetProperty("totalMagazines").GetInt32().Should().Be(0);
    }

    [Fact]
    public async Task ExportMagazineStatsAsJsonAsync_RivistaSenzaPagine_HaZeroPagineEZeroGiochi()
    {
        var rivista = new Magazine { Id = 5, Title = "Rivista", SourceFilePath = "r.pdf" };
        _riviste.Setup(m => m.GetAllAsync()).ReturnsAsync([rivista]);
        _riviste.Setup(m => m.GetWithPagesAndGamesAsync(5)).ReturnsAsync(rivista);

        var json = await _servizio.ExportMagazineStatsAsJsonAsync();

        using var documento = JsonDocument.Parse(json);
        var voce = documento.RootElement.GetProperty("magazines")[0];
        voce.GetProperty("totalPages").GetInt32().Should().Be(0);
        voce.GetProperty("totalGames").GetInt32().Should().Be(0);
    }

    private static Game NuovoGioco(int id, GameType tipo, string? titolo = "Titolo") => new()
    {
        Origine = Origine.Rivista,
        Id = id,
        GameType = tipo,
        Title = titolo,
        PositionX = 10,
        PositionY = 20,
        BoundingBoxWidth = 100,
        BoundingBoxHeight = 50,
        ClassificationConfidence = 0.5,
        ExtractedDate = new DateTime(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc),
        PageId = 4,
    };

    [Fact]
    public async Task ExportGamesAsJsonAsync_ConGiochi_ElencaCampiECalcolaIlTotale()
    {
        _giochi.Setup(g => g.GetAllAsync()).ReturnsAsync([NuovoGioco(1, GameType.Crossword), NuovoGioco(2, GameType.Rebus)]);

        var json = await _servizio.ExportGamesAsJsonAsync();

        using var documento = JsonDocument.Parse(json);
        documento.RootElement.GetProperty("totalGames").GetInt32().Should().Be(2);
        var primo = documento.RootElement.GetProperty("games")[0];
        primo.GetProperty("id").GetInt32().Should().Be(1);
        primo.GetProperty("title").GetString().Should().Be("Titolo");
        primo.GetProperty("position").GetProperty("positionX").GetInt32().Should().Be(10);
        primo.GetProperty("boundingBox").GetProperty("width").GetInt32().Should().Be(100);
        primo.GetProperty("pageId").GetInt32().Should().Be(4);
    }

    [Fact]
    public async Task ExportGamesAsCsvAsync_ConGiochi_ScriveUnaRigaPerGiocoConCampiTraVirgolette()
    {
        _giochi.Setup(g => g.GetAllAsync()).ReturnsAsync([NuovoGioco(1, GameType.Crossword, "Un \"titolo\""), NuovoGioco(2, GameType.Rebus, "=1+1")]);

        var csv = await _servizio.ExportGamesAsCsvAsync();

        var righe = csv.Split(["\r\n", "\n"], StringSplitOptions.RemoveEmptyEntries);
        righe.Should().HaveCount(4);
        righe[2].Should().StartWith("1,\"Crossword\",\"Un \"\"titolo\"\"\"");
        righe[2].Should().Contain(",0.5,2026-01-02T03:04:05.0000000Z,4");
        righe[3].Should().Contain("\"'=1+1\"");
    }

    [Fact]
    public async Task ExportMagazineStatsAsJsonAsync_ConPagineEGiochi_RaggruppaPerTipo()
    {
        var rivista = new Magazine { Id = 5, Title = "Rivista", SourceFilePath = "r.pdf", IssueNumber = "12", Year = 2026 };
        var pagina1 = new Page { Id = 1, MagazineId = 5, PageNumber = 1 };
        pagina1.Games.Add(NuovoGioco(1, GameType.Crossword));
        pagina1.Games.Add(NuovoGioco(2, GameType.Crossword));
        var pagina2 = new Page { Id = 2, MagazineId = 5, PageNumber = 2 };
        pagina2.Games.Add(NuovoGioco(3, GameType.Rebus));
        rivista.Pages.Add(pagina1);
        rivista.Pages.Add(pagina2);
        _riviste.Setup(m => m.GetAllAsync()).ReturnsAsync([rivista]);
        _riviste.Setup(m => m.GetWithPagesAndGamesAsync(5)).ReturnsAsync(rivista);

        var json = await _servizio.ExportMagazineStatsAsJsonAsync();

        using var documento = JsonDocument.Parse(json);
        var voce = documento.RootElement.GetProperty("magazines")[0];
        voce.GetProperty("totalPages").GetInt32().Should().Be(2);
        voce.GetProperty("totalGames").GetInt32().Should().Be(3);
        var perTipo = voce.GetProperty("gamesByType").EnumerateArray()
            .ToDictionary(e => e.GetProperty("gameType").GetString()!, e => e.GetProperty("count").GetInt32());
        perTipo.Should().BeEquivalentTo(new Dictionary<string, int> { ["Crossword"] = 2, ["Rebus"] = 1 });
    }

    [Fact]
    public async Task SaveToFileAsync_ScriveIlContenuto()
    {
        var percorso = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".json");
        try
        {
            await _servizio.SaveToFileAsync("{\"ok\":true}", percorso);

            (await File.ReadAllTextAsync(percorso)).Should().Be("{\"ok\":true}");
        }
        finally
        {
            File.Delete(percorso);
        }
    }
}
