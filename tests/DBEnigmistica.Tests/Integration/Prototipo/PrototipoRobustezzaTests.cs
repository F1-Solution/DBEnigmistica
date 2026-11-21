using System.Text.Json;
using System.Text.Json.Nodes;
using DBEnigmistica.Application.Creazione;
using DBEnigmistica.Application.FornitoreAi;
using DBEnigmistica.Tests.Unit.Creazione;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace DBEnigmistica.Tests.Integration.Prototipo;

/// <summary>Esiti della revisione 1b.1: incroci, concorrenza, annullamento, limiti, varianti e permessi fini.</summary>
[Collection(PostgresCollection.Name)]
[Trait("Category", "Integration")]
public class PrototipoRobustezzaTests(PostgresFixture fixture) : PrototipoTestBase(fixture)
{
    private sealed class FornitoreCheAspetta : IFornitoreAiCreazione
    {
        public TaskCompletionSource Iniziato { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public string IdFornitore => "finto";

        public string Modello => "modello-finto";

        public async Task<RispostaCreazione> CreaAsync(RichiestaCreazione richiesta, CancellationToken cancellationToken = default)
        {
            Iniziato.SetResult();
            await Task.Delay(Timeout.Infinite, cancellationToken);
            throw new InvalidOperationException("irraggiungibile");
        }
    }

    /// <summary>L'AI risponde e subito dopo la richiesta viene annullata: la risposta va comunque salvata.</summary>
    private sealed class FornitoreCheRispondeEPoiAnnulla(FornitoreCreazioneFinto interno, CancellationTokenSource cts) : IFornitoreAiCreazione
    {
        public string IdFornitore => interno.IdFornitore;

        public string Modello => interno.Modello;

        public async Task<RispostaCreazione> CreaAsync(RichiestaCreazione richiesta, CancellationToken cancellationToken = default)
        {
            var risposta = await interno.CreaAsync(richiesta, cancellationToken);
            await cts.CancelAsync();
            return risposta;
        }
    }

    private static (DescrittoreTipologia Descrittore, SchemaCaselle Caselle, string Brief) Cruciverba() =>
    (
        DescrittoreTipologia.Leggi(FileDelRepository.Leggi("tipologie", "_meta", "esempi", "descrittore", "valido-esempio-cruciverba.v1.json")),
        SchemaCaselle.Leggi(FileDelRepository.Esempio("schema-cruciverba", "valido-9x9.json")),
        JsonDocument.Parse(FileDelRepository.Esempio("prompt", "esempio-cruciverba-9x9.atteso.json")).RootElement.GetProperty("brief"u8).GetRawText());

    // Cambia una lettera della griglia: lo schema resta valido, ma la griglia non coincide più con le soluzioni.
    private static string ConIncrocioSbagliato()
    {
        var nodo = JsonNode.Parse(FileDelRepository.Esempio("risposta", "valido-esempio-cruciverba-9x9.json"))!;
        var celle = nodo["dati"]!["griglia"]!["celle"]!.AsArray();
        var riga = celle[1]!.GetValue<string>();
        celle[1] = (riga[0] == 'Z' ? 'Y' : 'Z') + riga[1..];
        return nodo.ToJsonString();
    }

    private async Task<Candidato> UnValidoAperto(ServiziPrototipo s, FornitoreCreazioneFinto finto)
    {
        finto.Riuscita(RispostaCrittografia());
        var c = (await s.Genera.EseguiAsync(BriefCrittografia())).Valido!;
        await s.Revisione.ProssimoAsync("crittografia-breve");
        return c;
    }

    // 1
    [Fact]
    public async Task Cruciverba_ConIncrociIncoerenti_SiSalvaDaRivedereConAvvisiNonSiRigenera_EAlAccettaSiRifiuta()
    {
        var (descrittore, caselle, brief) = Cruciverba();
        var finto = new FornitoreCreazioneFinto().Riuscita(ConIncrocioSbagliato());
        await using var s = Servizi(finto, new CatalogoInMemoria(descrittore), new GeneratoriFinti(caselle));

        var esito = await s.Genera.EseguiAsync(brief);

        var c = esito.Candidati.Should().ContainSingle("l'incrocio sbagliato non si scarta né si rigenera da solo").Subject;
        c.Stato.Should().Be(StatoCandidato.DaRivedere);
        JsonNode.DeepEquals(JsonNode.Parse(c.ContenutoGenerato!), JsonNode.Parse(ConIncrocioSbagliato())).Should().BeTrue("il contenuto resta come l'ha restituito il modello");
        (await s.Revisione.ControlliAsync(c)).Should().Contain(a => a.Contains("non coincidono"));
        await s.Revisione.ProssimoAsync(descrittore.Id);
        var accetta = () => s.Revisione.RegistraEsitoAsync(c.Id, new EsitoRichiesto("accettato"));
        await accetta.Should().ThrowAsync<RichiestaNonValidaException>().WithMessage("*non coincidono*");
        var scarto = await s.Revisione.RegistraEsitoAsync(c.Id, new EsitoRichiesto("scartato", "incroci_incoerenti"));
        scarto.Stato.Should().Be(StatoCandidato.Scartato);
    }

    // M1
    [Fact]
    public async Task Logico_ConIndiziNonCoerentiConIlDisegno_SalvaEAccettaDanno422()
    {
        await using var s = Servizi();
        var c = (await s.Genera.EseguiAsync(FileDelRepository.Esempio("brief", "valido-nonogramma-10x15.json"))).Valido!;
        await s.Revisione.ProssimoAsync("nonogramma");
        var contenuto = JsonNode.Parse(c.ContenutoGenerato!)!;
        contenuto["dati"]!["indizi_righe"]![0]!["blocchi"] = "15 15";
        var rotto = contenuto.ToJsonString();

        var salva = () => s.Revisione.SalvaVersioneAsync(c.Id, rotto);
        var accetta = () => s.Revisione.RegistraEsitoAsync(c.Id, new EsitoRichiesto("accettato", ContenutoFinale: rotto));

        await salva.Should().ThrowAsync<RichiestaNonValidaException>().WithMessage("*indizi delle righe*");
        await accetta.Should().ThrowAsync<RichiestaNonValidaException>().WithMessage("*indizi delle righe*");
        (await s.Revisione.ControlliAsync(c)).Should().BeEmpty("il contenuto generato è coerente");
    }

    [Fact]
    public async Task Sudoku_ConIndiziInsufficienti_SalvaDa422PerPiuSoluzioni()
    {
        await using var s = Servizi();
        var c = (await s.Genera.EseguiAsync(FileDelRepository.Esempio("brief", "valido-sudoku.json"))).Valido!;
        await s.Revisione.ProssimoAsync("sudoku");
        var contenuto = JsonNode.Parse(c.ContenutoGenerato!)!;
        var celle = contenuto["dati"]!["griglia"]!["celle"]!.AsArray();
        celle[0] = ".........";
        celle[1] = ".........";
        celle[2] = ".........";

        var salva = () => s.Revisione.SalvaVersioneAsync(c.Id, contenuto.ToJsonString());

        await salva.Should().ThrowAsync<RichiestaNonValidaException>().WithMessage("*più di una soluzione*");
    }

    // L2
    [Fact]
    public async Task Prossimo_NonSovrascriveUnEsitoConclusoNelFrattempo()
    {
        var finto = new FornitoreCreazioneFinto().Riuscita(RispostaCrittografia());
        await using var s = Servizi(finto);
        var c = (await s.Genera.EseguiAsync(BriefCrittografia())).Valido!;
        await using var altra = new Npgsql.NpgsqlConnection(ConnectionString);
        await altra.OpenAsync();
        await using var tx = await altra.BeginTransactionAsync();
        await using (var blocca = new Npgsql.NpgsqlCommand("SELECT 1 FROM prototipo.candidati WHERE id = @id FOR UPDATE", altra, tx))
        {
            blocca.Parameters.AddWithValue("id", c.Id);
            await blocca.ExecuteScalarAsync();
        }

        var prossimo = s.Revisione.ProssimoAsync("crittografia-breve");
        await Task.Delay(500);
        prossimo.IsCompleted.Should().BeFalse("la lettura attende il lucchetto di riga");
        await using (var scarta = new Npgsql.NpgsqlCommand(
                         "UPDATE prototipo.candidati SET stato = 'scartato', motivo_scarto = 'altro', revisione_conclusa_il = now() WHERE id = @id", altra, tx))
        {
            scarta.Parameters.AddWithValue("id", c.Id);
            await scarta.ExecuteNonQueryAsync();
        }

        await tx.CommitAsync();

        (await prossimo).Should().BeNull();
        (await ScalarAsync<string>("SELECT stato FROM prototipo.candidati")).Should().Be(StatoCandidato.Scartato);
    }

    // 2
    [Fact]
    public async Task DueAccettazioniInParallelo_ProduconoUnSoloEvento()
    {
        var finto = new FornitoreCreazioneFinto();
        await using var a = Servizi(finto);
        var c = await UnValidoAperto(a, finto);
        await using var b = Servizi(finto);

        var esiti = await Task.WhenAll(
            Prova(() => a.Revisione.RegistraEsitoAsync(c.Id, new EsitoRichiesto("accettato"))),
            Prova(() => b.Revisione.RegistraEsitoAsync(c.Id, new EsitoRichiesto("accettato"))));

        esiti.Count(e => e is null).Should().Be(1);
        esiti.Single(e => e is not null).Should().BeOfType<ConflittoCreazioneException>();
        await using var lettura = Servizi();
        (await lettura.Registro.LeggiInOrdineAsync().CountAsync(e => e.Tipo == "esito_revisione")).Should().Be(1);
    }

    [Fact]
    public async Task AccettaEScartaInParallelo_LoStatoFinaleNonSiRibalta()
    {
        var finto = new FornitoreCreazioneFinto();
        await using var a = Servizi(finto);
        var c = await UnValidoAperto(a, finto);
        await using var b = Servizi(finto);

        var esiti = await Task.WhenAll(
            Prova(() => a.Revisione.RegistraEsitoAsync(c.Id, new EsitoRichiesto("accettato"))),
            Prova(() => b.Revisione.RegistraEsitoAsync(c.Id, new EsitoRichiesto("scartato", "altro"))));

        esiti.Count(e => e is null).Should().Be(1);
        var stato = await ScalarAsync<string>("SELECT stato FROM prototipo.candidati");
        var eventi = await ScalarAsync<string>("SELECT dati FROM registro.eventi WHERE tipo = 'esito_revisione'");
        JsonDocument.Parse(eventi).RootElement.GetProperty("esito"u8).GetString().Should().Be(stato);
    }

    private static async Task<Exception?> Prova(Func<Task> azione)
    {
        try
        {
            await azione();
            return null;
        }
        catch (Exception ex)
        {
            return ex;
        }
    }

    // 4
    [Fact]
    public async Task Annullamento_PrimaDellaRisposta_SegnaErroreDiGenerazione()
    {
        var fornitore = new FornitoreCheAspetta();
        await using var s = Servizi(fornitore);
        using var cts = new CancellationTokenSource();
        var generazione = s.Genera.EseguiAsync(BriefCrittografia(), cts.Token);
        await fornitore.Iniziato.Task.WaitAsync(TimeSpan.FromSeconds(30));

        await cts.CancelAsync();
        var azione = async () => await generazione;

        await azione.Should().ThrowAsync<OperationCanceledException>();
        (await ScalarAsync<string>("SELECT stato FROM prototipo.candidati")).Should().Be(StatoCandidato.ErroreGenerazione);
    }

    [Fact]
    public async Task Annullamento_DopoLaRispostaDellAi_SalvaComunqueChiamataEdEvento()
    {
        using var cts = new CancellationTokenSource();
        var finto = new FornitoreCreazioneFinto().Riuscita(RispostaCrittografia());
        await using var s = Servizi(new FornitoreCheRispondeEPoiAnnulla(finto, cts));

        var esito = await s.Genera.EseguiAsync(BriefCrittografia(), cts.Token);

        esito.Valido.Should().NotBeNull();
        (await ScalarAsync<long>("SELECT count(*) FROM prototipo.chiamate_ai")).Should().Be(1);
        (await ScalarAsync<long>("SELECT count(*) FROM registro.eventi WHERE tipo = 'chiamata_ai'")).Should().Be(1);
    }

    // 9
    [Fact]
    public async Task LimiteRaggiunto_NonCreaUnCandidatoDiErrore()
    {
        var finto = new FornitoreCreazioneFinto().Riuscita(RispostaCrittografia());
        var limitato = new FornitoreAiCreazioneConLimite(finto, 1, new ContatoreChiamateAi());
        await using var s = Servizi(limitato);
        await s.Genera.EseguiAsync(BriefCrittografia());

        var seconda = () => s.Genera.EseguiAsync(BriefCrittografia());

        await seconda.Should().ThrowAsync<LimiteEsecuzioneRaggiuntoException>();
        (await ScalarAsync<long>("SELECT count(*) FROM prototipo.candidati")).Should().Be(1);
    }

    [Fact]
    public async Task RegoleInBozza_DannoUnaRichiestaNonValidaNonUnErroreInterno_ESenzaCandidato()
    {
        await using var s = Servizi(new FornitoreCreazioneFinto(), consentiBozza: false);

        var azione = () => s.Genera.EseguiAsync(BriefCrittografia());

        await azione.Should().ThrowAsync<RichiestaNonValidaException>().WithMessage("*bozza*");
        (await ScalarAsync<long>("SELECT count(*) FROM prototipo.candidati")).Should().Be(0);
    }

    // 8
    [Fact]
    public async Task ClicConcorrenti_HannoVariantiEImpronteDistinte()
    {
        var finto = new FornitoreCreazioneFinto().Riuscita(RispostaCrittografia()).Riuscita(RispostaCrittografia());
        await using var a = Servizi(finto);
        await using var b = Servizi(finto);

        await Task.WhenAll(a.Genera.EseguiAsync(BriefCrittografia()), b.Genera.EseguiAsync(BriefCrittografia()));

        finto.Richieste.Select(r => r.ImprontaRichiesta).Distinct().Should().HaveCount(2);
        var varianti = await a.Repository.BriefDellaTipologiaAsync("crittografia-breve");
        varianti.Select(v => JsonDocument.Parse(v).RootElement.GetProperty("variante"u8).GetInt32()).Order().Should().Equal(1, 2);
    }

    // 10
    [Fact]
    public async Task TransazioneFallita_PulisceIlTracciamentoENonRiprovaLeRigheSbagliate()
    {
        await using var s = Servizi();
        var transazione = new DBEnigmistica.Infrastructure.Prototipo.TransazionePrototipo(s.Contesto);
        var sbagliato = new Candidato { Id = Guid.CreateVersion7(), Tipologia = "sudoku", VersioneTipologia = 0, Famiglia = "logico", OrdineGenerazione = 1, GeneratoIl = DateTime.UtcNow, Modo = "algoritmo", Origine = "rivista", Brief = "{}", Seme = 1, VersioneGeneratore = "logici-1" };

        var azione = () => transazione.EseguiAsync(async () =>
        {
            s.Contesto.Candidati.Add(sbagliato);
            await s.Contesto.SaveChangesAsync();
            return true;
        });

        await azione.Should().ThrowAsync<DbUpdateException>();
        s.Contesto.ChangeTracker.Entries().Should().BeEmpty();
        var dopo = async () => await s.Contesto.SaveChangesAsync();
        await dopo.Should().NotThrowAsync();
    }

    // 11
    [Fact]
    public async Task NotaTroppoLunga_Da422NonUnErroreDelDatabase()
    {
        var finto = new FornitoreCreazioneFinto();
        await using var s = Servizi(finto);
        var c = await UnValidoAperto(s, finto);

        var azione = () => s.Revisione.RegistraEsitoAsync(c.Id, new EsitoRichiesto("scartato", "altro", new string('x', 1001)));

        await azione.Should().ThrowAsync<RichiestaNonValidaException>().WithMessage("*nota*");
    }

    // 12
    [Fact]
    public async Task ChiamateAi_SoloLaRispostaGrezzaSiAggiorna()
    {
        var finto = new FornitoreCreazioneFinto();
        await using var s = Servizi(finto);
        await UnValidoAperto(s, finto);

        (await SqlStateAsync("UPDATE prototipo.chiamate_ai SET risposta_grezza = NULL", "dbe_creazione")).Should().BeNull();
        (await SqlStateAsync("UPDATE prototipo.chiamate_ai SET durata_ms = 0", "dbe_creazione")).Should().Be("42501");
        (await SqlStateAsync("UPDATE prototipo.chiamate_ai SET risposta_sha256 = NULL", "dbe_creazione")).Should().Be("42501");
    }

    // 13
    [Fact]
    public async Task Prompt_NonContieneSentinelleInNessunCampoDelleRivisteNeInNessunaFamiglia()
    {
        var sentinelle = new[] { "SENT-TITOLO-aa1", "SENT-OCR-bb2", "SENT-GIOCO-cc3", "SENT-DESC-dd4", "SENT-SOL-ee5", "SENT-NOTE-ff6" };
        var (magazineId, pagine) = await SeedMagazineWithPagesAsync(1, NewMagazine(title: sentinelle[0]));
        await ExecuteAsync("UPDATE riviste.pages SET \"OcrText\" = @t WHERE \"Id\" = @id", P("t", sentinelle[1]), P("id", pagine[0]));
        var gameId = await SeedGameAsync(pagine[0], g =>
        {
            g.Title = sentinelle[2];
            g.Description = sentinelle[3];
            g.Solution = sentinelle[4];
            g.SolutionNotes = sentinelle[5];
        });
        gameId.Should().BeGreaterThan(0);
        magazineId.Should().BeGreaterThan(0);
        var (cruciverba, caselle, briefCruciverba) = Cruciverba();
        var anagramma = DescrittoreTipologia.Leggi(FileDelRepository.Leggi("tipologie", "_meta", "esempi", "descrittore", "valido-esempio-anagramma.v1.json"));
        var modello = ModelloPromptCreazione.Carica(RisorseCreazione.PromptCreazione);
        var casi = new List<RichiestaCreazione>
        {
            modello.Compila(Descrittore("crittografia-breve"), BriefCreazione.Leggi(BriefCrittografia()), null, true),
            modello.Compila(Descrittore("rebus"), BriefCreazione.Leggi(FileDelRepository.Esempio("brief", "valido-rebus.json")).ConVariante(1), null, true),
            modello.Compila(anagramma, BriefCreazione.Leggi(FileDelRepository.Esempio("brief", "valido-anagramma-tema.json")).ConVariante(1), null, true),
            modello.Compila(cruciverba, BriefCreazione.Leggi(briefCruciverba), caselle, true),
        };

        casi.Should().HaveCount(4);
        foreach (var richiesta in casi)
        {
            foreach (var sentinella in sentinelle)
                (richiesta.Prompt + richiesta.JsonSchema + richiesta.BriefJson).Should().NotContain(sentinella, richiesta.Tipologia);
        }
    }
}
