using DBEnigmistica.AI.Services;
using DBEnigmistica.Application.FornitoreAi;
using DBEnigmistica.Infrastructure.Services.Registrazioni;
using FluentAssertions;

namespace DBEnigmistica.Tests.Unit.FornitoreAi;

public class DecoratoriFornitoreAiTests : IDisposable
{
    private const string Impronta1 = "1111111111111111111111111111111111111111111111111111111111111111";
    private const string Impronta2 = "2222222222222222222222222222222222222222222222222222222222222222";

    private readonly string _cartella = Path.Combine(Path.GetTempPath(), "dbe-registrazioni-" + Guid.NewGuid().ToString("N"));

    public void Dispose()
    {
        if (Directory.Exists(_cartella))
            Directory.Delete(_cartella, recursive: true);
    }

    private static RichiestaEstrazionePagina Richiesta(string impronta = Impronta1, string prompt = "v1", int dpi = 200) =>
        new(impronta, [1, 2, 3], dpi, "prompt", prompt, "{}");

    private IArchivioRegistrazioni Archivio() => new ArchivioRegistrazioniFileSystem(_cartella);

    private static FornitoreAiRiproduci Riproduci(FornitoreAiFinto finto, IArchivioRegistrazioni archivio, IValidatoreSchemaJson? validatore = null) =>
        new(finto.IdFornitore, finto.Modello, archivio, new InterpreteRispostaClaudeCode(), validatore ?? new ValidatoreSempre([]));

    [Fact]
    public async Task Riproduci_SenzaRegistrazione_FallisceConErroreEsplicitoEZeroChiamate()
    {
        var finto = new FornitoreAiFinto();

        var chiamata = () => Riproduci(finto, Archivio()).EstraiPaginaAsync(Richiesta());

        (await chiamata.Should().ThrowAsync<RegistrazioneMancanteException>()).Which.Message.Should().Contain(Impronta1);
        finto.Chiamate.Should().Be(0);
    }

    [Fact]
    public async Task RegistraPoiRiproduci_RestituisceLaStessaRispostaSenzaNuoveChiamate()
    {
        var finto = new FornitoreAiFinto();
        var archivio = Archivio();
        var originale = await new FornitoreAiRegistra(finto, archivio).EstraiPaginaAsync(Richiesta());

        var riprodotta = await Riproduci(finto, archivio).EstraiPaginaAsync(Richiesta());

        riprodotta.Esito.Should().Be(EsitoEstrazione.Riuscita);
        riprodotta.Json.Should().Be("{\"giochi\":[]}");
        riprodotta.Chiamate.Should().BeEmpty("la riproduzione non fa chiamate");
        finto.Chiamate.Should().Be(1);
        originale.Chiamate.Should().HaveCount(1);
    }

    [Fact]
    public async Task Riproduci_ConservaIMetadatiDellaRegistrazione()
    {
        var finto = new FornitoreAiFinto { ChiamatePerPagina = 2 };
        var archivio = Archivio();
        await new FornitoreAiRegistra(finto, archivio).EstraiPaginaAsync(Richiesta());

        var riprodotta = await Riproduci(finto, archivio).EstraiPaginaAsync(Richiesta());

        riprodotta.Metadati.Should().NotBeNull();
        riprodotta.Metadati!.DurataMs.Should().Be(10);
        riprodotta.Metadati.Costo.Should().Be(1.0m);
        riprodotta.Metadati.CostoValuta.Should().Be("USD");
        riprodotta.Metadati.CostoTipo.Should().Be("indicativo");
        riprodotta.Metadati.TokenIngresso.Should().Be(2);
    }

    [Fact]
    public async Task Registra_ScriveIlFileNelPercorsoEConIlContenutoDelProgetto()
    {
        var finto = new FornitoreAiFinto();
        await new FornitoreAiRegistra(finto, Archivio()).EstraiPaginaAsync(Richiesta());

        var percorso = Path.Combine(_cartella, "finto", "modello-finto", "v1", Impronta1 + ".json");

        File.Exists(percorso).Should().BeTrue();
        var testo = await File.ReadAllTextAsync(percorso);
        testo.Should().NotContain("\r");
        using var json = System.Text.Json.JsonDocument.Parse(testo);
        json.RootElement.EnumerateObject().Select(p => p.Name).Should().Equal(
            "versione_formato", "chiave", "registrata_il", "dpi", "esito", "risposta_grezza", "risposta_sha256", "metadati");
        json.RootElement.GetProperty("versione_formato").GetInt32().Should().Be(1);
        json.RootElement.GetProperty("dpi").GetInt32().Should().Be(200);
        json.RootElement.GetProperty("esito").GetString().Should().Be("riuscita");
        json.RootElement.GetProperty("chiave").GetProperty("impronta_pagina").GetString().Should().Be(Impronta1);
        json.RootElement.GetProperty("registrata_il").GetString().Should().MatchRegex(@"^\d{4}-\d\d-\d\dT\d\d:\d\d:\d\d\.\d{6}Z$");
        json.RootElement.GetProperty("metadati").GetProperty("costo").GetString().Should().Be("0.500000");
    }

    [Fact]
    public async Task Registra_SalvaAncheLeRisposteNonValide()
    {
        var finto = new FornitoreAiFinto { Esito = EsitoEstrazione.RispostaNonValida };
        var archivio = Archivio();
        await new FornitoreAiRegistra(finto, archivio).EstraiPaginaAsync(Richiesta());

        var registrata = await archivio.CaricaAsync(new ChiaveRegistrazione(Impronta1, "v1", finto.IdFornitore, finto.Modello));

        registrata.Should().NotBeNull();
        registrata!.Esito.Should().Be(EsitoEstrazione.RispostaNonValida);
    }

    [Fact]
    public async Task Registra_NonSalvaGliErrori()
    {
        var finto = new FornitoreAiFinto { Esito = EsitoEstrazione.ErroreDefinitivo };
        var archivio = Archivio();
        await new FornitoreAiRegistra(finto, archivio).EstraiPaginaAsync(Richiesta());

        (await archivio.CaricaAsync(new ChiaveRegistrazione(Impronta1, "v1", finto.IdFornitore, finto.Modello))).Should().BeNull();
    }

    [Fact]
    public async Task Riproduci_ConAltraVersioneDelPrompt_NonTrovaLaRegistrazione()
    {
        var finto = new FornitoreAiFinto();
        var archivio = Archivio();
        await new FornitoreAiRegistra(finto, archivio).EstraiPaginaAsync(Richiesta());

        var chiamata = () => Riproduci(finto, archivio).EstraiPaginaAsync(Richiesta(prompt: "v2"));

        await chiamata.Should().ThrowAsync<RegistrazioneMancanteException>();
    }

    [Fact]
    public async Task Riproduci_ConDpiDiversoDallaRegistrazione_ERifiutatoEDiceIDpi()
    {
        var finto = new FornitoreAiFinto();
        var archivio = Archivio();
        await new FornitoreAiRegistra(finto, archivio).EstraiPaginaAsync(Richiesta(dpi: 200));

        var chiamata = () => Riproduci(finto, archivio).EstraiPaginaAsync(Richiesta(dpi: 300));

        (await chiamata.Should().ThrowAsync<RegistrazioneNonValidaException>())
            .Which.Message.Should().Contain("200 DPI").And.Contain("300 DPI");
    }

    [Fact]
    public async Task Riproduci_ConRispostaModificata_FallisceConShaDiverso()
    {
        var finto = new FornitoreAiFinto();
        var archivio = Archivio();
        await new FornitoreAiRegistra(finto, archivio).EstraiPaginaAsync(Richiesta());
        var percorso = Path.Combine(_cartella, "finto", "modello-finto", "v1", Impronta1 + ".json");
        await File.WriteAllTextAsync(percorso, (await File.ReadAllTextAsync(percorso)).Replace("giochi", "giochX"));

        var chiamata = () => Riproduci(finto, archivio).EstraiPaginaAsync(Richiesta());

        (await chiamata.Should().ThrowAsync<RegistrazioneNonValidaException>()).Which.Message.Should().Contain("SHA-256");
    }

    [Theory]
    [InlineData("non json")]
    [InlineData("{\"versione_formato\":1}")]
    [InlineData("")]
    public async Task Riproduci_ConFileCorrotto_DaUnErroreChiaroENonUnaJsonException(string contenuto)
    {
        var finto = new FornitoreAiFinto();
        var percorso = Path.Combine(_cartella, "finto", "modello-finto", "v1", Impronta1 + ".json");
        Directory.CreateDirectory(Path.GetDirectoryName(percorso)!);
        await File.WriteAllTextAsync(percorso, contenuto);

        var chiamata = () => Riproduci(finto, Archivio()).EstraiPaginaAsync(Richiesta());

        (await chiamata.Should().ThrowAsync<RegistrazioneNonValidaException>()).Which.Message.Should().Contain(Impronta1);
    }

    [Fact]
    public async Task Riproduci_RifaLaValidazioneDelloSchemaComeDalVivo()
    {
        var finto = new FornitoreAiFinto();
        var archivio = Archivio();
        await new FornitoreAiRegistra(finto, archivio).EstraiPaginaAsync(Richiesta());

        var risposta = await Riproduci(finto, archivio, new ValidatoreSempre(["manca il campo x"])).EstraiPaginaAsync(Richiesta());

        risposta.Esito.Should().Be(EsitoEstrazione.RispostaNonValida);
        risposta.Messaggio.Should().Contain("manca il campo x");
        risposta.Json.Should().BeNull();
    }

    [Fact]
    public async Task Riproduci_ConTestoNonJson_DiceRispostaNonValida()
    {
        var finto = new FornitoreAiFinto { Grezza = "{\"type\":\"result\",\"is_error\":false,\"result\":\"ciao\"}" };
        var archivio = Archivio();
        await new FornitoreAiRegistra(finto, archivio).EstraiPaginaAsync(Richiesta());

        var risposta = await Riproduci(finto, archivio).EstraiPaginaAsync(Richiesta());

        risposta.Esito.Should().Be(EsitoEstrazione.RispostaNonValida);
    }

    [Theory]
    [InlineData("ABC")]
    [InlineData("../../../etc/passwd")]
    [InlineData("")]
    public void Chiave_ConImprontaNonValida_VieneRifiutata(string impronta)
    {
        var crea = () => new ChiaveRegistrazione(impronta, "v1", "claude-code", "claude-sonnet-5-5");

        crea.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("v1", "../claude", "modello")]
    [InlineData("v1", "claude-code", "..")]
    [InlineData("v1", "claude-code", "a/b")]
    [InlineData("v1", "Claude", "modello")]
    [InlineData("../v1", "claude-code", "modello")]
    [InlineData("v1", "claude-code", "modello\\x")]
    public void Chiave_ConComponentiPericolosi_VieneRifiutata(string versione, string fornitore, string modello)
    {
        var crea = () => new ChiaveRegistrazione(Impronta1, versione, fornitore, modello);

        crea.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Chiave_ConImprontaMaiuscola_LaNormalizzaInMinuscolo()
    {
        var chiave = new ChiaveRegistrazione(new string('A', 64), "v1", "claude-code", "claude-sonnet-5-5");

        chiave.ImprontaPagina.Should().Be(new string('a', 64));
    }

    [Fact]
    public async Task Limite_DiUnaChiamata_RifiutaLaPaginaSuccessiva()
    {
        var finto = new FornitoreAiFinto();
        var contatore = new ContatoreChiamateAi();
        var limitato = new FornitoreAiConLimite(finto, 1, contatore);

        await limitato.EstraiPaginaAsync(Richiesta());
        var seconda = () => limitato.EstraiPaginaAsync(Richiesta(Impronta2));

        (await seconda.Should().ThrowAsync<LimiteEsecuzioneRaggiuntoException>())
            .Which.Message.Should().Be("Limite dell'esecuzione raggiunto");
        finto.Chiamate.Should().Be(1);
        contatore.Conteggio.Should().Be(1);
    }

    [Fact]
    public async Task Limite_ContaOgniChiamataRealeDiUnaPaginaConRetryECorrezioni()
    {
        var finto = new FornitoreAiFinto { ChiamatePerPagina = 3 };
        var contatore = new ContatoreChiamateAi();
        var limitato = new FornitoreAiConLimite(finto, 3, contatore);

        await limitato.EstraiPaginaAsync(Richiesta());

        contatore.Conteggio.Should().Be(3);
        var seconda = () => limitato.EstraiPaginaAsync(Richiesta(Impronta2));
        await seconda.Should().ThrowAsync<LimiteEsecuzioneRaggiuntoException>();
    }

    [Fact]
    public async Task Limite_Assente_ContaSenzaRifiutare()
    {
        var finto = new FornitoreAiFinto();
        var contatore = new ContatoreChiamateAi();
        var limitato = new FornitoreAiConLimite(finto, null, contatore);

        for (var i = 0; i < 5; i++)
            await limitato.EstraiPaginaAsync(Richiesta());

        contatore.Conteggio.Should().Be(5);
    }

    [Fact]
    public async Task RegistroChiamate_ConservaLaChiamataDiOgniRisposta()
    {
        var finto = new FornitoreAiFinto();
        var registro = new RegistroChiamateAiInMemoria();
        var decorato = new FornitoreAiConRegistroChiamate(finto, registro);

        await decorato.EstraiPaginaAsync(Richiesta(Impronta1));
        await decorato.EstraiPaginaAsync(Richiesta(Impronta2));

        registro.Chiamate.Should().HaveCount(2);
        registro.Chiamate[0].ImprontaPagina.Should().NotEqual(registro.Chiamate[1].ImprontaPagina);
    }

    private sealed class ValidatoreSempre(IReadOnlyList<string> errori) : IValidatoreSchemaJson
    {
        public IReadOnlyList<string> Valida(string json, string jsonSchema) => errori;
    }
}
