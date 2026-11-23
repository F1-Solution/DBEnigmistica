using System.Text.Json;
using DBEnigmistica.Application.Creazione;
using DBEnigmistica.Application.FornitoreAi;
using DBEnigmistica.Tests.Unit.Creazione;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace DBEnigmistica.Tests.Integration.Prototipo;

/// <summary>Casi d'uso del prototipo su PostgreSQL con il ruolo dbe_creazione e un Fornitore AI finto: nessuna chiamata di rete.</summary>
[Collection(PostgresCollection.Name)]
[Trait("Category", "Integration")]
public class PrototipoIntegrationTests(PostgresFixture fixture) : PrototipoTestBase(fixture)
{
    private async Task<List<DBEnigmistica.Domain.Registro.EventoRegistro>> EventiAsync()
    {
        await using var s = Servizi();
        return await s.Registro.LeggiInOrdineAsync().ToListAsync();
    }

    [Fact]
    public async Task GeneraConAi_SalvaUnCandidatoValidoConLaChiamataEIlSuoEvento()
    {
        var finto = new FornitoreCreazioneFinto().Riuscita(RispostaCrittografia());
        await using var s = Servizi(finto);

        var esito = await s.Genera.EseguiAsync(BriefCrittografia());

        var c = esito.Valido!;
        c.Stato.Should().Be(StatoCandidato.DaRivedere);
        c.OrdineGenerazione.Should().Be(1);
        c.Modo.Should().Be("ai");
        c.VersionePrompt.Should().StartWith("creazione-v1-");
        c.ImprontaRichiesta.Should().Equal(finto.Richieste[0].ImprontaByte());
        JsonDocument.Parse(c.Brief).RootElement.GetProperty("variante"u8).GetInt32().Should().Be(1);
        var chiamate = await s.Repository.ChiamateAsync(c.Id);
        chiamate.Should().ContainSingle().Which.RispostaGrezza.Should().Contain("RISPOSTA-GREZZA-DI-PROVA");
        var eventi = await EventiAsync();
        eventi.Should().ContainSingle(e => e.Tipo == "chiamata_ai");
        eventi.Single().Dati.Should().Contain(c.Id.ToString("D")).And.NotContain("RISPOSTA-GREZZA").And.NotContain("CENA");
        eventi.Single().Autore.Should().Be("motore-creazione");
    }

    [Fact]
    public async Task RispostaNonValida_SiSalvaComeTale_SiRigeneraFinoATreEPoiSiFerma()
    {
        var finto = new FornitoreCreazioneFinto().NonValida().NonValida().NonValida();
        await using var s = Servizi(finto);

        var esito = await s.Genera.EseguiAsync(BriefCrittografia());

        esito.Valido.Should().BeNull();
        esito.Messaggio.Should().NotBeNull();
        esito.Candidati.Select(c => c.Stato).Should().OnlyContain(x => x == StatoCandidato.NonValidoSchema);
        esito.Candidati.Select(c => JsonDocument.Parse(c.Brief).RootElement.GetProperty("variante"u8).GetInt32()).Should().Equal(1, 2, 3);
        finto.Richieste.Select(r => r.ImprontaRichiesta).Distinct().Should().HaveCount(3, "ogni variante cambia il prompt");
        (await s.Repository.ProssimoDaRivedereAsync("crittografia-breve")).Should().BeNull("i non validi non entrano nella revisione");
    }

    [Fact]
    public async Task NonValidoPoiValido_FermaLaRigenerazioneAlPrimoValido()
    {
        var finto = new FornitoreCreazioneFinto().NonValida().Riuscita(RispostaCrittografia());
        await using var s = Servizi(finto);

        var esito = await s.Genera.EseguiAsync(BriefCrittografia());

        esito.Candidati.Select(c => c.Stato).Should().Equal(StatoCandidato.NonValidoSchema, StatoCandidato.DaRivedere);
        esito.Candidati.Select(c => c.OrdineGenerazione).Should().Equal(1, 2);
    }

    [Fact]
    public async Task FinestraEsaurita_SalvaErroreDiGenerazioneENonRigenera()
    {
        var finto = new FornitoreCreazioneFinto().FinestraEsaurita();
        await using var s = Servizi(finto);

        var esito = await s.Genera.EseguiAsync(BriefCrittografia());

        esito.Candidati.Should().ContainSingle().Which.Stato.Should().Be(StatoCandidato.ErroreGenerazione);
        finto.Richieste.Should().HaveCount(1);
    }

    [Fact]
    public async Task BriefNonValidoOTipologiaSconosciuta_NonCreaNulla()
    {
        await using var s = Servizi();

        var nonValido = () => s.Genera.EseguiAsync(FileDelRepository.Esempio("brief", "non-valido-vincolo-a-capo.json"));
        var sconosciuta = () => s.Genera.EseguiAsync(FileDelRepository.Esempio("brief", "valido-cruciverba-11x11.json"));

        await nonValido.Should().ThrowAsync<RichiestaNonValidaException>();
        await sconosciuta.Should().ThrowAsync<NonTrovatoCreazioneException>();
        (await ScalarAsync<long>("SELECT count(*) FROM prototipo.candidati")).Should().Be(0);
    }

    [Fact]
    public async Task ErroreDelFornitore_NonLasciaIlCandidatoInGenerazione()
    {
        var finto = new FornitoreCreazioneFinto();
        await using var s = Servizi(finto);

        var azione = () => s.Genera.EseguiAsync(BriefCrittografia());

        await azione.Should().ThrowAsync<InvalidOperationException>();
        (await ScalarAsync<string>("SELECT stato FROM prototipo.candidati")).Should().Be(StatoCandidato.ErroreGenerazione);
    }

    [Fact]
    public async Task Sudoku_SiGeneraConIlGeneratoreECostaSemeEVersione()
    {
        await using var s = Servizi();

        var esito = await s.Genera.EseguiAsync(FileDelRepository.Esempio("brief", "valido-sudoku.json"));

        var c = esito.Valido!;
        c.Modo.Should().Be("algoritmo");
        c.Seme.Should().NotBeNull();
        c.VersioneGeneratore.Should().NotBeNull();
        c.VersionePrompt.Should().BeNull();
        (await s.Repository.ChiamateAsync(c.Id)).Should().BeEmpty();
    }

    [Fact]
    public async Task Cruciverba_UsaLoSchemaDelleCaselleEControllaLeLettereInGriglia()
    {
        var descrittore = DescrittoreTipologia.Leggi(FileDelRepository.Leggi("tipologie", "_meta", "esempi", "descrittore", "valido-esempio-cruciverba.v1.json"));
        var caselle = SchemaCaselle.Leggi(FileDelRepository.Esempio("schema-cruciverba", "valido-9x9.json"));
        var brief = JsonDocument.Parse(FileDelRepository.Esempio("prompt", "esempio-cruciverba-9x9.atteso.json")).RootElement.GetProperty("brief"u8).GetRawText();
        var finto = new FornitoreCreazioneFinto().Riuscita(FileDelRepository.Esempio("risposta", "valido-esempio-cruciverba-9x9.json"));
        await using var s = Servizi(finto, new CatalogoInMemoria(descrittore), new GeneratoriFinti(caselle));

        var esito = await s.Genera.EseguiAsync(brief);

        var c = esito.Valido!;
        c.SchemaCaselle.Should().NotBeNull();
        c.Seme.Should().Be(caselle.Seme);
        finto.Richieste[0].Prompt.Should().Contain("Parole orizzontali:");
    }

    private static async Task<Candidato[]> TreValidiAsync(ServiziPrototipo s)
    {
        var tre = new List<Candidato>();
        foreach (var _ in Enumerable.Range(0, 3))
            tre.Add((await s.Genera.EseguiAsync(BriefCrittografia())).Valido!);
        return [.. tre];
    }

    [Fact]
    public async Task Revisione_ServeICandidatiInOrdineSenzaSalti()
    {
        var finto = new FornitoreCreazioneFinto().Riuscita(RispostaCrittografia()).Riuscita(RispostaCrittografia()).Riuscita(RispostaCrittografia());
        await using var s = Servizi(finto);
        var tre = await TreValidiAsync(s);

        var primo = await s.Revisione.ProssimoAsync("crittografia-breve");
        var saltaAlTerzo = () => s.Revisione.RegistraEsitoAsync(tre[2].Id, new EsitoRichiesto("accettato"));
        var saltaAlSecondo = () => s.Revisione.SalvaVersioneAsync(tre[1].Id, RispostaCrittografia());

        primo!.Id.Should().Be(tre[0].Id);
        primo.Stato.Should().Be(StatoCandidato.InRevisione);
        await saltaAlTerzo.Should().ThrowAsync<ConflittoCreazioneException>();
        await saltaAlSecondo.Should().ThrowAsync<ConflittoCreazioneException>();

        await s.Revisione.RegistraEsitoAsync(tre[0].Id, new EsitoRichiesto("accettato"));
        (await s.Revisione.ProssimoAsync("crittografia-breve"))!.Id.Should().Be(tre[1].Id);
    }

    [Fact]
    public async Task ConInterruttoreSpento_GenerareConAiSiRifiutaSenzaCreareNulla()
    {
        const string messaggio = "Le chiamate dal vivo sono disattivate (prova).";
        await using var s = Servizi(new FornitoreAiCreazioneDisattivato(messaggio));

        var azione = () => s.Genera.EseguiAsync(BriefCrittografia());

        (await azione.Should().ThrowAsync<RichiestaNonValidaException>()).Which.Errori.Should().Contain(messaggio);
        (await s.Repository.BriefDellaTipologiaAsync("crittografia-breve")).Should().BeEmpty("il rifiuto avviene prima di creare un candidato");
    }

    [Fact]
    public async Task RegoleInBozza_InRiproduzione_LaGenerazioneFunziona()
    {
        var finto = new FornitoreAiCreazioneInRiproduzione(new FornitoreCreazioneFinto().Riuscita(RispostaCrittografia()));
        await using var s = Servizi(finto, consentiBozza: false);

        var esito = await s.Genera.EseguiAsync(BriefCrittografia());

        esito.Valido!.Stato.Should().Be(StatoCandidato.DaRivedere);
    }

    [Fact]
    public async Task RegoleInBozza_DalVivo_SiRifiutanoSenzaChiamate()
    {
        var finto = new FornitoreCreazioneFinto().Riuscita(RispostaCrittografia());
        await using var s = Servizi(finto, consentiBozza: false);

        var azione = () => s.Genera.EseguiAsync(BriefCrittografia());

        (await azione.Should().ThrowAsync<RichiestaNonValidaException>()).Which.Errori.Should().Contain(e => e.Contains("bozza", StringComparison.Ordinal));
        finto.Richieste.Should().BeEmpty();
    }

    [Fact]
    public async Task InterruttoreSpento_ConRegoleApprovate_RifiutaComunqueSenzaChiamate()
    {
        var approvato = Descrittore("crittografia-breve") with { Regole = Descrittore("crittografia-breve").Regole with { Approvata = true, ApprovateIl = "2026-12-01" } };
        await using var s = Servizi(new FornitoreAiCreazioneDisattivato("spento"), new CatalogoInMemoria(approvato), consentiBozza: false);

        var azione = () => s.Genera.EseguiAsync(BriefCrittografia());

        (await azione.Should().ThrowAsync<RichiestaNonValidaException>()).Which.Errori.Should().Contain("spento");
    }

    [Fact]
    public async Task ConInterruttoreSpento_IlSudokuSiGeneraEIlCandidatoSiRivede()
    {
        await using var s = Servizi(new FornitoreAiCreazioneDisattivato("spento"));

        var esito = await s.Genera.EseguiAsync(FileDelRepository.Esempio("brief", "valido-sudoku.json"));
        var prossimo = await s.Revisione.ProssimoAsync("sudoku");

        esito.Valido!.Modo.Should().Be("algoritmo");
        prossimo!.Id.Should().Be(esito.Valido.Id);
    }

    [Fact]
    public async Task ReviewTime_LeggeICandidatiDalDatabaseConIlRuoloDelPrototipo()
    {
        var finto = new FornitoreCreazioneFinto().Riuscita(RispostaCrittografia()).Riuscita(RispostaCrittografia());
        await using var s = Servizi(finto);
        var primo = (await s.Genera.EseguiAsync(BriefCrittografia())).Valido!;
        await s.Genera.EseguiAsync(BriefCrittografia());
        await s.Revisione.RegistraEsitoAsync(primo.Id, new EsitoRichiesto("accettato", TempoAttivoMs: 600_000));
        var configurazione = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["ConnectionStrings:DefaultConnection"] = ConnectionString }).Build();

        var candidati = await new DBEnigmistica.Infrastructure.Prototipo.LettoreCandidatiRapporto(configurazione).LeggiAsync();
        var rapporto = DBEnigmistica.Application.Valutazione.TempiRevisione.CalcolatoreTempiRevisione.Calcola(candidati, DateTimeOffset.UnixEpoch);

        var g = rapporto.Gruppi.Single(x => x.Gruppo == "crittografia_breve");
        g.Validi.Should().Be(2);
        g.Rivisti.Should().Be(1);
        g.Minuti.Mediana.Should().Be(10);
    }

    [Fact]
    public async Task SalvaTempo_SalvaTempoEUltimaAttivitaESoloInCrescita()
    {
        var finto = new FornitoreCreazioneFinto().Riuscita(RispostaCrittografia());
        await using var s = Servizi(finto);
        var c = (await s.Genera.EseguiAsync(BriefCrittografia())).Valido!;
        await s.Revisione.ProssimoAsync("crittografia-breve");
        var t1 = new DateTime(2026, 11, 24, 10, 0, 0, DateTimeKind.Utc);

        var salvato = await s.Revisione.SalvaTempoAsync(c.Id, 160_000, t1);
        var uguale = await s.Revisione.SalvaTempoAsync(c.Id, 160_000, t1.AddSeconds(15));
        var indietro = () => s.Revisione.SalvaTempoAsync(c.Id, 159_999, t1.AddSeconds(30));
        var negativo = () => s.Revisione.SalvaTempoAsync(c.Id, -1, t1);

        salvato.TempoAttivoMs.Should().Be(160_000);
        salvato.UltimaAttivitaIl.Should().Be(t1);
        uguale.UltimaAttivitaIl.Should().Be(t1.AddSeconds(15));
        await indietro.Should().ThrowAsync<RichiestaNonValidaException>().WithMessage("*non può diminuire*");
        await negativo.Should().ThrowAsync<RichiestaNonValidaException>();
        (await s.Repository.TrovaAsync(c.Id))!.TempoAttivoMs.Should().Be(160_000);
        (await EventiAsync()).Should().NotContain(e => e.Tipo == "esito_revisione", "il salvataggio periodico non scrive nel registro");
    }

    [Fact]
    public async Task SalvaTempo_SuUnCandidatoCheNonELProssimoODopoLEsito_DaConflitto()
    {
        var finto = new FornitoreCreazioneFinto().Riuscita(RispostaCrittografia()).Riuscita(RispostaCrittografia());
        await using var s = Servizi(finto);
        var primo = (await s.Genera.EseguiAsync(BriefCrittografia())).Valido!;
        var secondo = (await s.Genera.EseguiAsync(BriefCrittografia())).Valido!;
        var adesso = DateTime.UtcNow;

        var saltaAlSecondo = () => s.Revisione.SalvaTempoAsync(secondo.Id, 1000, adesso);
        var sconosciuto = () => s.Revisione.SalvaTempoAsync(Guid.NewGuid(), 1000, adesso);
        await saltaAlSecondo.Should().ThrowAsync<ConflittoCreazioneException>();
        await sconosciuto.Should().ThrowAsync<NonTrovatoCreazioneException>();

        await s.Revisione.RegistraEsitoAsync(primo.Id, new EsitoRichiesto("accettato", TempoAttivoMs: 5000));
        var dopoLEsito = () => s.Revisione.SalvaTempoAsync(primo.Id, 6000, adesso);
        await dopoLEsito.Should().ThrowAsync<ConflittoCreazioneException>();
    }

    [Fact]
    public async Task Modifica_DiDueCampi_SalvaLeDifferenzeEAccettaConUnEventoSenzaTesto()
    {
        var finto = new FornitoreCreazioneFinto().Riuscita(RispostaCrittografia());
        await using var s = Servizi(finto);
        var c = (await s.Genera.EseguiAsync(BriefCrittografia())).Valido!;
        await s.Revisione.ProssimoAsync("crittografia-breve");
        var modificato = RispostaCrittografia("PRANZO DEL SOVRANO", "pranzo = pasto; sovrano = re");

        var salvato = await s.Revisione.SalvaVersioneAsync(c.Id, modificato);
        var accettato = await s.Revisione.RegistraEsitoAsync(c.Id, new EsitoRichiesto("accettato", TempoAttivoMs: 734000));

        var differenze = JsonDocument.Parse(salvato.Differenze!).RootElement;
        differenze.GetArrayLength().Should().Be(2);
        differenze[0].GetProperty("percorso"u8).GetString().Should().Be("/dati/esposto");
        differenze[0].GetProperty("prima"u8).GetString().Should().Be("CENA SOVRANO");
        differenze[0].GetProperty("dopo"u8).GetString().Should().Be("PRANZO DEL SOVRANO");
        accettato.Stato.Should().Be(StatoCandidato.Accettato);
        var esito = (await EventiAsync()).Single(e => e.Tipo == "esito_revisione");
        esito.Autore.Should().Be("proprietario");
        esito.Entita.Should().Be("candidato_prototipo");
        var dati = JsonDocument.Parse(esito.Dati).RootElement;
        dati.GetProperty("campi_modificati"u8).GetInt32().Should().Be(2);
        dati.GetProperty("tempo_attivo_ms"u8).GetInt64().Should().Be(734000);
        dati.GetProperty("esito"u8).GetString().Should().Be("accettato");
        dati.GetProperty("contenuto_finale_sha256"u8).GetString().Should().HaveLength(64);
        esito.Dati.Should().NotContain("PRANZO").And.NotContain("CENA").And.NotContain("pasto");
        (await VerificaRegistroAsync()).Integra.Should().BeTrue();
    }

    [Fact]
    public async Task Scarto_RichiedeUnMotivoDellaListaESalvaLaNotaSoloNelloSchemaPrototipo()
    {
        var finto = new FornitoreCreazioneFinto().Riuscita(RispostaCrittografia());
        await using var s = Servizi(finto);
        var c = (await s.Genera.EseguiAsync(BriefCrittografia())).Valido!;

        var senzaMotivo = () => s.Revisione.RegistraEsitoAsync(c.Id, new EsitoRichiesto("scartato"));
        var motivoStrano = () => s.Revisione.RegistraEsitoAsync(c.Id, new EsitoRichiesto("scartato", "boh"));
        await senzaMotivo.Should().ThrowAsync<RichiestaNonValidaException>();
        await motivoStrano.Should().ThrowAsync<RichiestaNonValidaException>();

        var scartato = await s.Revisione.RegistraEsitoAsync(c.Id, new EsitoRichiesto("scartato", "soluzione_errata", "NOTA-RISERVATA"));

        scartato.Stato.Should().Be(StatoCandidato.Scartato);
        scartato.MotivoScarto.Should().Be("soluzione_errata");
        var evento = (await EventiAsync()).Single(e => e.Tipo == "esito_revisione");
        evento.Dati.Should().Contain("soluzione_errata").And.NotContain("NOTA-RISERVATA");
    }

    [Fact]
    public async Task Accetta_ConContenutoNonValido_Rifiuta()
    {
        var finto = new FornitoreCreazioneFinto().Riuscita(RispostaCrittografia());
        await using var s = Servizi(finto);
        var c = (await s.Genera.EseguiAsync(BriefCrittografia())).Valido!;
        var rotto = RispostaCrittografia().Replace("\"soluzione\": \"PASTORE\"", "\"soluzione\": null", StringComparison.Ordinal);

        var azione = () => s.Revisione.RegistraEsitoAsync(c.Id, new EsitoRichiesto("accettato", ContenutoFinale: rotto));

        await azione.Should().ThrowAsync<RichiestaNonValidaException>();
        (await s.Repository.TrovaAsync(c.Id))!.Stato.Should().NotBe(StatoCandidato.Accettato);
    }

    [Fact]
    public async Task RispostaGrezza_SiCancellaEIlRegistroRestaIntegro()
    {
        var finto = new FornitoreCreazioneFinto().Riuscita(RispostaCrittografia());
        await using var s = Servizi(finto);
        var c = (await s.Genera.EseguiAsync(BriefCrittografia())).Valido!;
        await s.Revisione.RegistraEsitoAsync(c.Id, new EsitoRichiesto("accettato"));

        var toccate = await s.Revisione.CancellaRisposteGrezzeAsync(null);

        toccate.Should().Be(1);
        var chiamata = (await s.Repository.ChiamateAsync(c.Id)).Single();
        chiamata.RispostaGrezza.Should().BeNull();
        chiamata.RispostaSha256.Should().HaveCount(32);
        (await VerificaRegistroAsync()).Integra.Should().BeTrue();
    }

    [Fact]
    public async Task InGenerazioneOltreLAttesa_PassaAErroreAllAvvio()
    {
        await using var s = Servizi();
        await ExecuteAsync(
            "INSERT INTO prototipo.candidati (id, tipologia, versione_tipologia, famiglia, ordine_generazione, generato_il, modo, origine, brief, seme, versione_generatore, stato, tempo_attivo_ms) "
            + "VALUES (gen_random_uuid(), 'sudoku', 0, 'logico', 1, now() - interval '1 hour', 'algoritmo', 'originale', '{}', 1, 'logici-1', 'in_generazione', 0)");

        var riparati = await s.Revisione.RiparaGenerazioniInterrotteAsync();

        riparati.Should().Be(1);
        (await ScalarAsync<string>("SELECT stato FROM prototipo.candidati")).Should().Be(StatoCandidato.ErroreGenerazione);
    }

    [Fact]
    public async Task Prompt_NonContieneStringheSentinellaScritteInRiviste()
    {
        const string sentinella = "SENTINELLA-RIVISTA-7f3a91";
        await SeedMagazineWithPagesAsync(1, NewMagazine(title: sentinella));
        var finto = new FornitoreCreazioneFinto().Riuscita(RispostaCrittografia());
        await using var s = Servizi(finto);

        await s.Genera.EseguiAsync(BriefCrittografia());

        (await ScalarAsync<string>("SELECT \"Title\" FROM riviste.magazines")).Should().Be(sentinella);
        finto.Richieste.Should().OnlyContain(r => !r.Prompt.Contains(sentinella) && !r.JsonSchema.Contains(sentinella) && !r.BriefJson.Contains(sentinella));
    }

    [Fact]
    public async Task RuoloCreazione_NonLeggeRivisteEILContestoDelPrototipoLoStaAFare()
    {
        await using var s = Servizi();

        var lettura = async () => await s.Contesto.Database.ExecuteSqlRawAsync("SELECT 1 FROM riviste.magazines");

        var errore = (await lettura.Should().ThrowAsync<PostgresException>()).Which;
        errore.SqlState.Should().Be("42501");
        (await SqlStateAsync("SELECT 1 FROM riviste.magazines", "dbe_creazione")).Should().Be("42501");
    }

    [Fact]
    public async Task Permessi_DelloSchemaPrototipo()
    {
        const string select = "SELECT 1 FROM prototipo.candidati";
        (await SqlStateAsync(select, "dbe_creazione")).Should().BeNull();
        (await SqlStateAsync(select, "dbe_estrazione")).Should().Be("42501");
        (await SqlStateAsync(select, "dbe_originalita")).Should().Be("42501");
        (await SqlStateAsync("DELETE FROM prototipo.candidati", "dbe_creazione")).Should().Be("42501");
        (await SqlStateAsync("SELECT 1 FROM prototipo.chiamate_ai", "dbe_estrazione")).Should().Be("42501");
    }

    [Fact]
    public async Task SchemaPrototipo_NonHaColonnePerImmagini()
    {
        var colonne = await ScalarAsync<long>(
            "SELECT count(*) FROM information_schema.columns WHERE table_schema = 'prototipo' "
            + "AND (column_name ~* '(immagin|image|png|foto|figura)' OR data_type = 'bytea' AND column_name NOT IN ('impronta_richiesta', 'risposta_sha256'))");

        colonne.Should().Be(0);
    }

    [Fact]
    public async Task Vincoli_DelloSchema_RifiutanoOrigineDiversaEStatiIncoerenti()
    {
        const string base1 = "INSERT INTO prototipo.candidati (id, tipologia, versione_tipologia, famiglia, ordine_generazione, generato_il, modo, origine, brief, seme, versione_generatore, stato, tempo_attivo_ms) VALUES (gen_random_uuid(), 'sudoku', 0, 'logico', {0}, now(), 'algoritmo', '{1}', '{{}}', 1, 'logici-1', '{2}', 0)";

        (await SqlStateAsync(string.Format(base1, 1, "rivista", "in_generazione"))).Should().Be("23514");
        (await SqlStateAsync(string.Format(base1, 2, "originale", "da_rivedere"))).Should().Be("23514", "un valido deve avere il contenuto generato");
        (await SqlStateAsync(string.Format(base1, 3, "originale", "accettato"))).Should().Be("23514");
        (await SqlStateAsync(string.Format(base1, 4, "originale", "in_generazione"))).Should().BeNull();
        (await SqlStateAsync(string.Format(base1, 4, "originale", "in_generazione"))).Should().Be("23505", "l'ordine è unico per Tipologia");
    }
}
