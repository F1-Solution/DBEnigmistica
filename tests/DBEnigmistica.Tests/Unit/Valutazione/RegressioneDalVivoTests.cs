using DBEnigmistica.AI.Services;
using DBEnigmistica.Application.FornitoreAi;
using DBEnigmistica.Application.Valutazione;
using DBEnigmistica.Tests.Fixtures.ClaudeFinto;
using FluentAssertions;
using static DBEnigmistica.Tests.Unit.Valutazione.ServiziDiProva;

namespace DBEnigmistica.Tests.Unit.Valutazione;

/// <summary>La modalità dal vivo provata solo con l'eseguibile finto di Claude Code: nessuna chiamata di rete.</summary>
[Collection("AmbienteProcesso")]
public sealed class RegressioneDalVivoTests : IDisposable
{
    private static readonly string[] Pagine = ["s01", "s02", "s03", "s04", "s05", "s06"];

    private readonly string _radice = Path.Combine(Path.GetTempPath(), "dbe-vivo-" + Guid.NewGuid().ToString("N"));

    public void Dispose()
    {
        if (Directory.Exists(_radice))
            Directory.Delete(_radice, recursive: true);
    }

    // Set senza Linea di base: le condizioni dal vivo (fornitore claude-code) non sono quelle registrate.
    private string SetSenzaLineaDiBase()
    {
        CopiaCartella(SetVersionato, _radice);
        File.Delete(Path.Combine(_radice, "linea-di-base.json"));
        return _radice;
    }

    private static ClaudeFintoIstanza FintoCheRispondeConLeRisposteDelSet() =>
        ClaudeFintoFixture.Crea([.. Pagine.Select(id => new PassoClaudeFinto(
            "{\"type\":\"result\",\"is_error\":false,\"result\":\"\",\"structured_output\":"
            + File.ReadAllText(Path.Combine(SetVersionato, "risposte", id + ".json")) + "}"))]);

    private static FornitoreClaudeCode Adattatore(ClaudeFintoIstanza finto) =>
        new(new ImpostazioniClaudeCode(finto.Eseguibile, "claude-finto", TimeSpan.FromSeconds(120), 1, TimeSpan.FromSeconds(1)), new ValidatoreSchemaJson());

    [Fact]
    public async Task DalVivo_ConLEseguibileFinto_FaUnaChiamataPerPaginaEMisuraCometeLaRiproduzione()
    {
        var cartella = SetSenzaLineaDiBase();
        using var finto = FintoCheRispondeConLeRisposteDelSet();
        var contatore = new ContatoreChiamateAi();
        var fornitore = new FornitoreAiConLimite(Adattatore(finto), 10, contatore);

        var vivo = await Servizio().EseguiAsync(Parametri(cartella, dalVivo: true), fornitore);
        var riprodotto = await Servizio().EseguiAsync(Parametri(cartella), Riproduci(cartella));

        vivo.ChiamateAi.Should().Be(6);
        contatore.Conteggio.Should().Be(6);
        finto.Invocazioni.Should().HaveCount(6);
        // Le metriche di qualità sono uguali; il tempo dipende dal processo e resta fuori dal confronto.
        SenzaConsumi(vivo.Rapporto).Should().BeEquivalentTo(SenzaConsumi(riprodotto.Rapporto));
        vivo.Rapporto.Condizioni.Fornitore.Should().Be("claude-code");
    }

    private static Dictionary<string, MetricheGruppo> SenzaConsumi(RapportoRegressione rapporto) =>
        rapporto.Gruppi.ToDictionary(g => g.Key, g => g.Value with { CostoMedio = null, PagineConCosto = 0, TempoMedioMs = null, PagineConTempo = 0 });

    [Fact]
    public async Task DalVivo_ImmagineEPromptArrivanoAlFornitore()
    {
        var cartella = SetSenzaLineaDiBase();
        using var finto = FintoCheRispondeConLeRisposteDelSet();

        await Servizio().EseguiAsync(Parametri(cartella, dalVivo: true), Adattatore(finto));

        var prima = finto.Invocazioni[0];
        prima.LunghezzaPng.Should().BeGreaterThan(1000, "in modalità dal vivo la pagina si rende in PNG");
        prima.Stdin.Should().Contain("### anagramma").And.Contain("### cruciverba-classico");
        finto.Invocazioni.Select(i => i.Stdin).Should().Contain(s => s.Contains("CRUCIVERBA DI PROVA", StringComparison.Ordinal), "il testo nativo della pagina s01 entra nel prompt");
    }

    [Fact]
    public async Task DalVivo_ALimiteRaggiunto_SiFermaSenzaAltreChiamate()
    {
        var cartella = SetSenzaLineaDiBase();
        using var finto = FintoCheRispondeConLeRisposteDelSet();
        var fornitore = new FornitoreAiConLimite(Adattatore(finto), 2, new ContatoreChiamateAi());

        var azione = () => Servizio().EseguiAsync(Parametri(cartella, dalVivo: true), fornitore);

        await azione.Should().ThrowAsync<LimiteEsecuzioneRaggiuntoException>();
        finto.Invocazioni.Should().HaveCount(2);
    }

    [Fact]
    public async Task DalVivo_ConUnErroreDelFornitore_ErroreEsplicitoSenzaRapporto()
    {
        var cartella = SetSenzaLineaDiBase();
        using var finto = ClaudeFintoFixture.Crea(new PassoClaudeFinto("", "errore generico", Exit: 1));

        var azione = () => Servizio().EseguiAsync(Parametri(cartella, dalVivo: true), Adattatore(finto));

        await azione.Should().ThrowAsync<ValutazioneException>().WithMessage("*s01*");
    }
}
