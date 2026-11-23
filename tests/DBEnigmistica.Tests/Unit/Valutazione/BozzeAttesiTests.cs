using System.Text.Json;
using DBEnigmistica.AI.Services;
using DBEnigmistica.Application.Valutazione;
using DBEnigmistica.Tests.Unit.Creazione;
using FluentAssertions;
using static DBEnigmistica.Tests.Unit.Valutazione.ServiziDiProva;

namespace DBEnigmistica.Tests.Unit.Valutazione;

public sealed class BozzeAttesiTests : IDisposable
{
    private readonly string _radice = Path.Combine(Path.GetTempPath(), "dbe-bozze-" + Guid.NewGuid().ToString("N"));

    public void Dispose()
    {
        if (Directory.Exists(_radice))
            Directory.Delete(_radice, recursive: true);
    }

    private string SetSenzaAttesi(params string[] mantieni)
    {
        CopiaCartella(SetVersionato, _radice);
        foreach (var file in Directory.GetFiles(Path.Combine(_radice, "attesi")).Where(f => !mantieni.Contains(Path.GetFileNameWithoutExtension(f))))
            File.Delete(file);
        File.Delete(Path.Combine(_radice, "linea-di-base.json"));
        return _radice;
    }

    private static string Atteso(string cartella, string id) => File.ReadAllText(Path.Combine(cartella, "attesi", id + ".json"));

    [Fact]
    public async Task Bozza_ScriveUnFilePerPaginaConformeALloSchemaDelRisultatoAtteso()
    {
        var cartella = SetSenzaAttesi();

        var esito = await Bozze().BozzaAsync(cartella, "finto", "finto-1", 200, sovrascrivi: false);

        esito.Scritte.Should().Equal("s01", "s02", "s03", "s04", "s05", "s06");
        var schema = FileDelRepository.Leggi("tipologie", "_meta", "risultato-atteso.schema.json");
        foreach (var id in esito.Scritte)
            new ValidatoreSchemaJson().Valida(Atteso(cartella, id), schema).Should().BeEmpty(id);
    }

    [Fact]
    public async Task Bozza_HaStatoBozzaEIndicaLaRegistrazioneDiOrigine()
    {
        var cartella = SetSenzaAttesi();

        await Bozze().BozzaAsync(cartella, "finto", "finto-1", 200, sovrascrivi: false);

        using var documento = JsonDocument.Parse(Atteso(cartella, "s02"));
        var revisione = documento.RootElement.GetProperty("revisione"u8);
        revisione.GetProperty("stato"u8).GetString().Should().Be("bozza");
        revisione.GetProperty("bozza_da"u8).GetProperty("versione_prompt"u8).GetString().Should().Be(Versione.VersionePrompt);
        revisione.GetProperty("bozza_da"u8).GetProperty("fornitore"u8).GetString().Should().Be("finto");
        revisione.GetProperty("corretto_da"u8).ValueKind.Should().Be(JsonValueKind.Null);
        documento.RootElement.GetProperty("giochi"u8).GetArrayLength().Should().Be(3);
    }

    [Fact]
    public async Task Bozza_UnGiocoDiTipologiaNonPrevista_HaSoloLaFamiglia()
    {
        var cartella = SetSenzaAttesi();

        await Bozze().BozzaAsync(cartella, "finto", "finto-1", 200, sovrascrivi: false);

        using var documento = JsonDocument.Parse(Atteso(cartella, "s06"));
        var proposta = documento.RootElement.GetProperty("giochi"u8)[1];
        proposta.GetProperty("stato_tipologia"u8).GetString().Should().Be("proposta");
        proposta.GetProperty("famiglia"u8).GetString().Should().Be("gioco-breve");
        proposta.TryGetProperty("dati"u8, out _).Should().BeFalse();
    }

    [Fact]
    public async Task Bozza_UnaBozzaLettaDalLettore_EFuoriDalleMetriche()
    {
        var cartella = SetSenzaAttesi();
        await Bozze().BozzaAsync(cartella, "finto", "finto-1", 200, sovrascrivi: false);

        var risultato = await Servizio().EseguiAsync(Parametri(cartella), Riproduci(cartella));

        risultato.Rapporto.Pagine.Should().OnlyContain(p => p.InBozza);
        risultato.Rapporto.Gruppi["totale"].Pagine.Should().Be(0);
    }

    [Fact]
    public async Task Bozza_UnFileEsistenteNonSiSovrascriveSenzaRichiestaEUnCorrettoMai()
    {
        var cartella = SetSenzaAttesi("s01");
        var corretto = Atteso(cartella, "s01");

        var primo = await Bozze().BozzaAsync(cartella, "finto", "finto-1", 200, sovrascrivi: false);
        var secondo = await Bozze().BozzaAsync(cartella, "finto", "finto-1", 200, sovrascrivi: true);

        primo.Saltate.Should().Equal("s01");
        secondo.Saltate.Should().Contain("s01");
        Atteso(cartella, "s01").Should().Be(corretto);
    }

    [Fact]
    public async Task Bozza_UnaBozzaEsistenteSiSovrascriveSoloSuRichiesta()
    {
        var cartella = SetSenzaAttesi();
        await Bozze().BozzaAsync(cartella, "finto", "finto-1", 200, sovrascrivi: false);
        File.AppendAllText(Path.Combine(cartella, "attesi", "s02.json"), " ");

        var senza = await Bozze().BozzaAsync(cartella, "finto", "finto-1", 200, sovrascrivi: false);
        var conRichiesta = await Bozze().BozzaAsync(cartella, "finto", "finto-1", 200, sovrascrivi: true);

        senza.Scritte.Should().BeEmpty();
        conRichiesta.Scritte.Should().Contain("s02");
        Atteso(cartella, "s02").Should().NotEndWith(" ");
    }

    [Fact]
    public async Task Bozza_SenzaRegistrazione_ErroreEsplicitoENessunFileScritto()
    {
        var cartella = SetSenzaAttesi();
        Directory.Delete(Path.Combine(cartella, "registrazioni"), recursive: true);

        var azione = () => Bozze().BozzaAsync(cartella, "finto", "finto-1", 200, sovrascrivi: false);

        await azione.Should().ThrowAsync<ValutazioneException>().WithMessage("*Registrazione mancante*");
        Directory.GetFiles(Path.Combine(cartella, "attesi")).Should().BeEmpty();
    }
}
