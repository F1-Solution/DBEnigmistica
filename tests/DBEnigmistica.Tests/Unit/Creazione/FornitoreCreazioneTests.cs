using System.Text.Json;
using DBEnigmistica.AI.Configuration;
using DBEnigmistica.AI.Services;
using DBEnigmistica.Application.Creazione;
using DBEnigmistica.Application.FornitoreAi;
using DBEnigmistica.Console.Cli;
using DBEnigmistica.Infrastructure.Prototipo;
using DBEnigmistica.Tests.Fixtures.ClaudeFinto;
using DBEnigmistica.Tests.Integration.Prototipo;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DBEnigmistica.Tests.Unit.Creazione;

/// <summary>Adattatore, decoratori, Registrazioni e configurazione della creazione: eseguibile finto, nessuna chiamata di rete.</summary>
[Collection("AmbienteProcesso")]
public class FornitoreCreazioneTests : IDisposable
{
    private const string Modello = "claude-opus-5-5";

    private static readonly string Valido = FileDelRepository.Esempio("risposta", "valido-crittografia-sinonimica.json");

    private readonly string _cartella = Path.Combine(Path.GetTempPath(), "dbe-reg-creazione-" + Guid.NewGuid().ToString("N"));

    public void Dispose()
    {
        if (Directory.Exists(_cartella))
            Directory.Delete(_cartella, recursive: true);
    }

    private static string Uscita(string json) =>
        "{\"type\":\"result\",\"is_error\":false,\"result\":\"\",\"structured_output\":" + json
        + ",\"total_cost_usd\":0.5,\"usage\":{\"input_tokens\":10,\"output_tokens\":2}}";

    private static RichiestaCreazione Richiesta(string prompt = "Scrivi un gioco\nnuovo", string impronta = "")
    {
        var schema = DerivatoreSchemaCreazione.Deriva(
            DescrittoreTipologia.Leggi(FileDelRepository.Leggi("tipologie", "crittografia-breve", "v0.json")), null, null);
        return new RichiestaCreazione("crittografia-breve", 0, "{}", prompt, "creazione-v1-aaaaaaaaaaaa", schema);
    }

    private static FornitoreClaudeCodeCreazione Crea(ClaudeFintoIstanza finto, IValidatoreSchemaJson? validatore = null) =>
        new(
            new ImpostazioniClaudeCode(finto.Eseguibile, Modello, TimeSpan.FromSeconds(60), 3, TimeSpan.FromSeconds(1)),
            validatore ?? new ValidatoreSchemaJson(),
            null,
            (_, _) => Task.CompletedTask);

    [Fact]
    public async Task RispostaValida_RestituisceUnCandidatoConUsoCreazioneEImprontaDellaRichiesta()
    {
        using var finto = ClaudeFintoFixture.Crea(new PassoClaudeFinto(Uscita(Valido)));
        var richiesta = Richiesta();

        var risposta = await Crea(finto).CreaAsync(richiesta);

        risposta.Esito.Should().Be(EsitoEstrazione.Riuscita);
        JsonDocument.Parse(risposta.Json!).RootElement.GetProperty("tipologia"u8).GetString().Should().Be("crittografia-breve");
        var chiamata = risposta.Chiamate.Single();
        chiamata.Uso.Should().Be("creazione");
        chiamata.Modello.Should().Be(Modello);
        chiamata.ImprontaPagina.Should().Equal(Convert.FromHexString(richiesta.ImprontaRichiesta));
        chiamata.TestoNativoTroncato.Should().BeFalse();
        chiamata.RispostaGrezza.Should().Contain("structured_output");
        risposta.Metadati!.TokenIngresso.Should().Be(10);
    }

    [Fact]
    public async Task Avvio_SenzaImmagineNeStrumentiConIlPromptSuStdinEAmbientePulito()
    {
        using var finto = ClaudeFintoFixture.Crea(new PassoClaudeFinto(Uscita(Valido)));
        var chiave = Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY");
        Environment.SetEnvironmentVariable("ANTHROPIC_API_KEY", "chiave-di-prova");
        try
        {
            await Crea(finto).CreaAsync(Richiesta());
        }
        finally
        {
            Environment.SetEnvironmentVariable("ANTHROPIC_API_KEY", chiave);
        }

        var invocazione = finto.Invocazioni.Single();
        invocazione.Stdin.Should().Be("Scrivi un gioco\nnuovo");
        invocazione.ChiaveApi.Should().BeFalse();
        invocazione.File.Should().BeEmpty("nessuna immagine e nessun file nella cartella di lavoro");
        var argomenti = invocazione.Args.ToList();
        argomenti[argomenti.IndexOf("--tools") + 1].Should().BeEmpty("nessuno strumento abilitato");
        argomenti.Should().NotContain("--allowedTools").And.NotContain("Read");
        argomenti[argomenti.IndexOf("--model") + 1].Should().Be(Modello);
        argomenti.Should().Contain("--json-schema");
    }

    [Fact]
    public async Task RispostaNonConforme_ChiedeUnaCorrezioneEPoiDiceRispostaNonValida()
    {
        using var finto = ClaudeFintoFixture.Crea(new PassoClaudeFinto(Uscita("{\"x\":1}")));

        var risposta = await Crea(finto).CreaAsync(Richiesta());

        risposta.Esito.Should().Be(EsitoEstrazione.RispostaNonValida);
        risposta.Chiamate.Select(c => (c.Tentativo, c.Correzione)).Should().Equal((1, false), (2, true));
        finto.Invocazioni.Should().HaveCount(2);
        finto.Invocazioni[1].Stdin.Should().Contain("non rispetta lo schema");
    }

    [Fact]
    public async Task CorrezioneRiuscita_DiceRiuscita()
    {
        using var finto = ClaudeFintoFixture.Crea(new PassoClaudeFinto(Uscita("{\"x\":1}")), new PassoClaudeFinto(Uscita(Valido)));

        var risposta = await Crea(finto).CreaAsync(Richiesta());

        risposta.Esito.Should().Be(EsitoEstrazione.Riuscita);
        risposta.Chiamate.Should().HaveCount(2);
    }

    [Fact]
    public async Task FinestraEsaurita_EsitoDedicato()
    {
        var limite = "{\"type\":\"result\",\"is_error\":true,\"result\":\"You've hit your session limit · resets 3:45pm\"}";
        using var finto = ClaudeFintoFixture.Crea(new PassoClaudeFinto(limite, Exit: 1));

        var risposta = await Crea(finto).CreaAsync(Richiesta());

        risposta.Esito.Should().Be(EsitoEstrazione.FinestraEsaurita);
        risposta.Chiamate.Single().Esito.Should().Be(EsitoEstrazione.FinestraEsaurita);
    }

    [Fact]
    public async Task ErroreTransitorio_RitentaEPoiRiesce()
    {
        var sovraccarico = "{\"type\":\"result\",\"is_error\":true,\"result\":\"API Error: 529 Overloaded\"}";
        using var finto = ClaudeFintoFixture.Crea(new PassoClaudeFinto(sovraccarico, Exit: 1), new PassoClaudeFinto(Uscita(Valido)));

        var risposta = await Crea(finto).CreaAsync(Richiesta());

        risposta.Chiamate.Select(c => c.Esito).Should().Equal(EsitoEstrazione.ErroreTransitorio, EsitoEstrazione.Riuscita);
    }

    [Fact]
    public async Task EseguibileCmd_ERifiutato()
    {
        var fornitore = new FornitoreClaudeCodeCreazione(
            new ImpostazioniClaudeCode("claude.cmd", Modello, TimeSpan.FromSeconds(5)), new ValidatoreSchemaJson());

        var risposta = await fornitore.CreaAsync(Richiesta());

        risposta.Esito.Should().Be(EsitoEstrazione.ErroreDefinitivo);
    }

    [Fact]
    public void Impronta_E_Sha256DiPromptByteZeroESchema()
    {
        var richiesta = new RichiestaCreazione("t", 0, "{}", "ab", "creazione-v1-aaaaaaaaaaaa", "{}");

        var atteso = Convert.ToHexStringLower(System.Security.Cryptography.SHA256.HashData([(byte)'a', (byte)'b', 0, (byte)'{', (byte)'}']));

        richiesta.ImprontaRichiesta.Should().Be(atteso);
    }

    [Fact]
    public async Task Limite_DiUnaChiamata_RifiutaLaSecondaConLimiteRaggiunto()
    {
        var finto = new FornitoreCreazioneFinto().Riuscita(Valido).Riuscita(Valido);
        var limitato = new FornitoreAiCreazioneConLimite(finto, 1, new ContatoreChiamateAi());

        await limitato.CreaAsync(Richiesta());
        var seconda = () => limitato.CreaAsync(Richiesta());

        await seconda.Should().ThrowAsync<LimiteEsecuzioneRaggiuntoException>();
        finto.Richieste.Should().HaveCount(1);
    }

    [Fact]
    public async Task Riproduci_SenzaRegistrazione_ErroreEsplicitoEZeroChiamate()
    {
        var archivio = new ArchivioRegistrazioniCreazioneFileSystem(_cartella);
        var riproduci = new FornitoreAiCreazioneRiproduci("claude-code", Modello, archivio, new InterpreteRispostaClaudeCode(), new ValidatoreSchemaJson());

        var azione = () => riproduci.CreaAsync(Richiesta());

        await azione.Should().ThrowAsync<RegistrazioneMancanteException>();
    }

    [Fact]
    public async Task RegistraPoiRiproduci_DaLaStessaRispostaSenzaChiamate()
    {
        using var finto = ClaudeFintoFixture.Crea(new PassoClaudeFinto(Uscita(Valido)));
        var archivio = new ArchivioRegistrazioniCreazioneFileSystem(_cartella);
        var richiesta = Richiesta();
        var registra = new FornitoreAiCreazioneRegistra(Crea(finto), archivio);
        await registra.CreaAsync(richiesta);
        var riproduci = new FornitoreAiCreazioneRiproduci("claude-code", Modello, archivio, new InterpreteRispostaClaudeCode(), new ValidatoreSchemaJson());

        var riprodotta = await riproduci.CreaAsync(richiesta);

        riprodotta.Esito.Should().Be(EsitoEstrazione.Riuscita);
        riprodotta.Chiamate.Should().BeEmpty();
        finto.Invocazioni.Should().HaveCount(1);
        File.Exists(Path.Combine(_cartella, "claude-code", Modello, richiesta.VersionePrompt, richiesta.ImprontaRichiesta + ".json")).Should().BeTrue();
    }

    [Fact]
    public async Task Riproduci_RifiutaUnaRegistrazioneModificataOPerUnaTipologiaDiversa()
    {
        using var finto = ClaudeFintoFixture.Crea(new PassoClaudeFinto(Uscita(Valido)));
        var archivio = new ArchivioRegistrazioniCreazioneFileSystem(_cartella);
        var richiesta = Richiesta();
        await new FornitoreAiCreazioneRegistra(Crea(finto), archivio).CreaAsync(richiesta);
        var riproduci = new FornitoreAiCreazioneRiproduci("claude-code", Modello, archivio, new InterpreteRispostaClaudeCode(), new ValidatoreSchemaJson());

        var altraTipologia = () => riproduci.CreaAsync(richiesta with { Tipologia = "rebus" });
        await altraTipologia.Should().ThrowAsync<RegistrazioneNonValidaException>().WithMessage("*crittografia-breve*");
        var percorso = Path.Combine(_cartella, "claude-code", Modello, richiesta.VersionePrompt, richiesta.ImprontaRichiesta + ".json");
        File.WriteAllText(percorso, File.ReadAllText(percorso).Replace("structured_output", "structured_outpuT", StringComparison.Ordinal));
        var modificata = () => riproduci.CreaAsync(richiesta);

        await modificata.Should().ThrowAsync<RegistrazioneNonValidaException>().WithMessage("*SHA-256*");
    }

    [Fact]
    public async Task RegistrazioneDelRepository_SiRiproduceByteConByte()
    {
        var atteso = JsonDocument.Parse(FileDelRepository.Esempio("prompt", "crittografia-sinonimica.atteso.json")).RootElement;
        var descrittore = DescrittoreTipologia.Leggi(FileDelRepository.Leggi("tipologie", "crittografia-breve", "v0.json"));
        var brief = BriefCreazione.Leggi(atteso.GetProperty("brief"u8).GetRawText());
        var richiesta = ModelloPromptCreazione.Carica(RisorseCreazione.PromptCreazione).Compila(descrittore, brief, null, consentiBozza: true);
        var archivio = new ArchivioRegistrazioniCreazioneFileSystem(FileDelRepository.Percorso("tests", "DBEnigmistica.Tests", "Fixtures", "RegistrazioniCreazione"));
        var riproduci = new FornitoreAiCreazioneRiproduci("claude-code", Modello, archivio, new InterpreteRispostaClaudeCode(), new ValidatoreSchemaJson());

        var risposta = await riproduci.CreaAsync(richiesta);

        risposta.Esito.Should().Be(EsitoEstrazione.Riuscita);
        risposta.Metadati!.TokenIngresso.Should().Be(2120);
    }

    [Fact]
    public void ModelloDiCreazione_SiLeggeDallaChiaveSeparata_EQuelloDiEstrazioneNonCambia()
    {
        using var host = CliHost.Build(
            HostRequirements.None,
            new CliStreams(new StringWriter(), new StringWriter()),
            b => b.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["FornitoreAi:Tipo"] = "claude-code",
                ["FornitoreAi:Modello"] = "claude-sonnet-5-5",
                ["FornitoreAi:Creazione:Modello"] = Modello,
                ["FornitoreAi:DalVivoAbilitato"] = "true",
            }));
        using var scope = host.Services.CreateScope();

        var creazione = scope.ServiceProvider.GetRequiredService<IFornitoreAiCreazione>();
        var estrazione = scope.ServiceProvider.GetRequiredService<IFornitoreAiEstrazione>();

        creazione.Modello.Should().Be(Modello);
        estrazione.Modello.Should().Be("claude-sonnet-5-5");
    }

    [Fact]
    public void ModelloPredefinito_DiCreazione_EOpusNelFileDiConfigurazione()
    {
        var configurazione = new ConfigurationBuilder().AddJsonFile(Path.Combine(AppContext.BaseDirectory, "appsettings.json")).Build();

        configurazione["FornitoreAi:Creazione:Modello"].Should().Be(Modello);
        configurazione["FornitoreAi:Modello"].Should().Be("claude-sonnet-5-5");
    }

    [Fact]
    public void ValidazioneDellaCreazione_ChiedeIlModelloEMessaggiConLaChiaveGiusta()
    {
        var opzioni = new FornitoreAiOptions { Tipo = "claude-code", Creazione = new CreazioneOptions { Registrazioni = new RegistrazioniOptions { Modalita = "riproduci" } } };

        var errori = ValidatoreOpzioniCreazione.Valida(opzioni, new AmbienteFornitoreAi());

        errori.Should().Contain(e => e.Contains("FornitoreAi__Creazione__Modello"));
        opzioni.Creazione.Modello = Modello;
        ValidatoreOpzioniCreazione.Valida(opzioni, new AmbienteFornitoreAi()).Should().ContainSingle().Which.Should().Contain("FornitoreAi__Creazione__Registrazioni__Cartella");
    }

    [Fact]
    public async Task Catalogo_LeggeIDescrittoriEIdPericolosiNonSiRisolvono()
    {
        var catalogo = new CatalogoTipologieFileSystem(FileDelRepository.Percorso("tipologie"));

        (await catalogo.TrovaAsync("rebus", 0))!.Famiglia.Should().Be("rebus");
        (await catalogo.TrovaAsync("rebus", 7)).Should().BeNull();
        (await catalogo.TrovaAsync("../_meta/esempi", 0)).Should().BeNull();
        (await catalogo.TrovaAsync("rebus/../sudoku", 0)).Should().BeNull();
    }

    [Fact]
    public void Differenze_UnaVoceInPiuOInMenoSiRiportaIntera_ELaGrigliaPerRiga()
    {
        const string prima = "{\"titolo\":null,\"dati\":{\"celle\":[\"AB\",\"CD\"],\"voci\":[{\"a\":1}]}}";
        const string dopo = "{\"titolo\":\"T\",\"dati\":{\"celle\":[\"AB\",\"XD\"],\"voci\":[{\"a\":1},{\"a\":2}]}}";

        var (json, numero) = Differenze.Calcola(prima, dopo);

        numero.Should().Be(3);
        var voci = JsonDocument.Parse(json).RootElement.EnumerateArray().Select(v => v.GetProperty("percorso"u8).GetString()).ToList();
        voci.Should().Equal("/titolo", "/dati/celle/1", "/dati/voci/1");
    }

    [Fact]
    public void Jcs_OrdinaLeChiaviEUsaGliInteriCompatti()
    {
        Jcs.Canonicalizza("{ \"b\": 2, \"a\": [1, \"x\\u00e8\", null, true], \"c\": 1.5 }").Should().Be("{\"a\":[1,\"xè\",null,true],\"b\":2,\"c\":1.5}");
        Jcs.Sha256Esadecimale("{\"a\":1}").Should().Be(Jcs.Sha256Esadecimale("{ \"a\" : 1 }"));
    }

    [Fact]
    public async Task Evento_ChiamataAi_HaLeChiaviNellOrdineDelContrattoESoloMetadati()
    {
        var finto = new FornitoreCreazioneFinto().Riuscita(Valido);
        var risposta = await finto.CreaAsync(Richiesta());
        var candidato = new Candidato { Id = Guid.CreateVersion7(), Tipologia = "crittografia-breve", VersioneTipologia = 0 };

        var evento = EventiCreazione.ChiamataAi(risposta.Chiamate[0], candidato);

        evento.Autore.Should().Be("motore-creazione");
        JsonDocument.Parse(evento.Dati).RootElement.EnumerateObject().Select(p => p.Name).Should().Equal(
            "versione", "uso", "fornitore", "modello", "versione_prompt", "impronta_richiesta", "tipologia", "versione_tipologia", "candidato_id",
            "tentativo", "correzione", "esito", "durata_ms", "token_ingresso", "token_uscita", "token_cache_creazione", "token_cache_lettura",
            "costo", "costo_valuta", "costo_tipo", "risposta_sha256");
        evento.Dati.Should().NotContain("RISPOSTA-GREZZA");
    }

    [Fact]
    public void Generatori_ProduconoContenutiValidiPerLoSchemaDeiDescrittori()
    {
        var validatore = new ValidatoreSchemaJson();
        var generatori = new GeneratoriAlgoritmici();
        foreach (var (id, brief) in new[]
                 {
                     ("sudoku", "valido-sudoku.json"), ("nonogramma", "valido-nonogramma-10x15.json"),
                 })
        {
            var descrittore = DescrittoreTipologia.Leggi(FileDelRepository.Leggi("tipologie", id, "v0.json"));
            var b = BriefCreazione.Leggi(FileDelRepository.Esempio("brief", brief)).ConVariante(1);

            var risultato = generatori.Genera(id, b, 42);

            validatore.Valida(risultato.ContenutoJson, DerivatoreSchemaCreazione.Deriva(descrittore, b, null)).Should().BeEmpty(id);
            generatori.Genera(id, b, 42).ContenutoJson.Should().Be(risultato.ContenutoJson, "stesso seme, stesso contenuto");
        }
    }

    [Fact]
    public void SchemaCaselle_DelGeneratore_RispettaLeRegoleEDelloSchemaDerivato()
    {
        var caselle = new GeneratoriAlgoritmici().GeneraSchemaCaselle(11, 7);

        caselle.Righe.Should().Be(11);
        caselle.Parole.Should().NotBeEmpty();
        SchemaCaselle.Leggi(caselle.ToJson()).Parole.Should().HaveCount(caselle.Parole.Count);
        var descrittore = DescrittoreTipologia.Leggi(FileDelRepository.Leggi("tipologie", "_meta", "esempi", "descrittore", "valido-esempio-cruciverba.v1.json"));
        DerivatoreSchemaCreazione.Deriva(descrittore, null, caselle).Length.Should().BeLessThan(DerivatoreSchemaCreazione.LunghezzaMassimaSchema + 1);
    }
}
