using System.Text.Json;
using DBEnigmistica.Infrastructure.Tipologie;
using DBEnigmistica.Tests.Unit.Tipologie;
using DBEnigmistica.Tipologie.Caricamento;
using DBEnigmistica.Tipologie.Generatore;
using DBEnigmistica.Tipologie.Modello;
using FluentAssertions;
using Npgsql;

namespace DBEnigmistica.Tests.Integration.Tipologie;

/// <summary>Scrittore e lettore delle tabelle per Tipologia: parametri, valori ostili salvati come dati, tipi, elenchi e rifiuti.</summary>
[Collection(PostgresCollection.Name)]
[Trait("Category", "Integration")]
public class ArchivioDatiTipologiaIntegrationTests : IntegrationTestBase
{
    private const string Ostile = "x'); DROP TABLE registro.eventi; --\"; DELETE FROM catalogo.giochi;";

    private static readonly ArchivioDatiTipologia Archivio = new();

    public ArchivioDatiTipologiaIntegrationTests(PostgresFixture fixture) : base(fixture)
    {
    }

    private static Descrittore Carica(string id) =>
        new CaricatoreDescrittori().CaricaCartella(PercorsiTipologie.Cartella).Descrittori.Single(d => d.Id == id && d.Versione == 1);

    private async Task<NpgsqlConnection> ConnessioneAsync()
    {
        var connessione = new NpgsqlConnection(ConnectionString);
        await connessione.OpenAsync();
        await new MigratoreTipologie().ApplicaAsync(connessione, FonteMigrazioniTipologie.DalleRisorse(), CancellationToken.None);
        return connessione;
    }

    private static JsonElement Json(string testo) => JsonDocument.Parse(testo).RootElement.Clone();

    private static string Anagramma(string esposto) =>
        JsonSerializer.Serialize(new { esposto, diagramma = "4 = 4", prima_parola = (string?)null, soluzione = "ROMA", autore = (string?)null });

    [Fact]
    public async Task Un_valore_ostile_si_salva_come_dato_e_non_cambia_il_database()
    {
        var gioco = await SeedGiocoOriginaleAsync();
        await using var connessione = await ConnessioneAsync();
        var anagramma = Carica("anagramma");

        await Archivio.ScriviAsync(connessione, null, anagramma, SchemaTipologia.Catalogo, gioco, Json(Anagramma(Ostile)), CancellationToken.None);

        var letto = await Archivio.LeggiAsync(connessione, null, anagramma, SchemaTipologia.Catalogo, gioco, CancellationToken.None);
        letto!["esposto"]!.GetValue<string>().Should().Be(Ostile);
        letto["soluzione"]!.GetValue<string>().Should().Be("ROMA");
        letto["autore"].Should().BeNull();
        (await ScalarAsync<long>("SELECT count(*) FROM information_schema.tables WHERE table_name = 'eventi' AND table_schema = 'registro'")).Should().Be(1);
        (await ScalarAsync<long>("SELECT count(*) FROM catalogo.giochi")).Should().Be(1);
    }

    [Fact]
    public async Task Un_cruciverba_con_elenco_e_griglia_si_rilegge_uguale()
    {
        var gioco = await SeedGiocoOriginaleAsync();
        await using var connessione = await ConnessioneAsync();
        var cruciverba = Carica("cruciverba-classico");
        var dati = Json(
            """
            {"righe":2,"colonne":2,"griglia":{"righe":2,"colonne":2,"celle":["A#","BC"],"numeri":[{"riga":1,"colonna":1,"numero":1}]},
             "autore":null,
             "definizioni":[{"numero":1,"direzione":"verticale","testo":"Prima","soluzione":null},{"numero":2,"direzione":"orizzontale","testo":"Seconda","soluzione":"BC"}]}
            """);

        await Archivio.ScriviAsync(connessione, null, cruciverba, SchemaTipologia.Catalogo, gioco, dati, CancellationToken.None);
        var letto = await Archivio.LeggiAsync(connessione, null, cruciverba, SchemaTipologia.Catalogo, gioco, CancellationToken.None);

        letto!["righe"]!.GetValue<int>().Should().Be(2);
        letto["griglia"]!["celle"]![1]!.GetValue<string>().Should().Be("BC");
        var definizioni = letto["definizioni"]!.AsArray();
        definizioni.Select(d => d!["testo"]!.GetValue<string>()).Should().Equal("Prima", "Seconda");
        definizioni[0]!["soluzione"].Should().BeNull();
        (await ScalarAsync<long>("SELECT count(*) FROM catalogo.gioco_cruciverba_classico__definizioni")).Should().Be(2);
    }

    [Fact]
    public async Task Senza_dati_la_lettura_da_null()
    {
        var gioco = await SeedGiocoOriginaleAsync();
        await using var connessione = await ConnessioneAsync();

        (await Archivio.LeggiAsync(connessione, null, Carica("anagramma"), SchemaTipologia.Catalogo, gioco, CancellationToken.None)).Should().BeNull();
    }

    [Fact]
    public async Task Una_scrittura_che_fallisce_a_meta_si_annulla_per_intero()
    {
        var gioco = await SeedGiocoOriginaleAsync();
        await using var connessione = await ConnessioneAsync();
        var cruciverba = Carica("cruciverba-classico");
        var dati = Json(
            """
            {"righe":1,"colonne":1,"griglia":{"righe":1,"colonne":1,"celle":["A"],"numeri":[]},"autore":null,
             "definizioni":[{"numero":1,"direzione":"diagonale","testo":"Direzione fuori dall'elenco","soluzione":null}]}
            """);

        var azione = () => Archivio.ScriviAsync(connessione, null, cruciverba, SchemaTipologia.Catalogo, gioco, dati, CancellationToken.None);

        (await azione.Should().ThrowAsync<PostgresException>()).Which.SqlState.Should().Be(PostgresErrorCodes.CheckViolation);
        (await ScalarAsync<long>("SELECT count(*) FROM catalogo.gioco_cruciverba_classico")).Should().Be(0);
    }

    [Fact]
    public async Task Tipi_sbagliati_chiavi_estranee_e_id_di_tipo_errato_sono_rifiutati_prima_di_scrivere()
    {
        var gioco = await SeedGiocoOriginaleAsync();
        await using var connessione = await ConnessioneAsync();
        var anagramma = Carica("anagramma");

        async Task ScriviAsync(string json, object id) =>
            await Archivio.ScriviAsync(connessione, null, anagramma, SchemaTipologia.Catalogo, id, Json(json), CancellationToken.None);

        var estranea = () => ScriviAsync(Anagramma("ok").Replace("\"autore\":null", "\"autore\":null,\"estraneo\":\"x\"", StringComparison.Ordinal), gioco);
        var tipoErrato = () => ScriviAsync(Anagramma("ok").Replace("\"esposto\":\"ok\"", "\"esposto\":12", StringComparison.Ordinal), gioco);
        var idErrato = () => ScriviAsync(Anagramma("ok"), 5);

        (await estranea.Should().ThrowAsync<ValoreTipologiaNonValidoException>()).Which.Percorso.Should().Be("estraneo");
        (await tipoErrato.Should().ThrowAsync<ValoreTipologiaNonValidoException>()).Which.Percorso.Should().Be("esposto");
        await idErrato.Should().ThrowAsync<ArgumentException>();
        (await ScalarAsync<long>("SELECT count(*) FROM catalogo.gioco_anagramma")).Should().Be(0);
    }

    [Fact]
    public async Task Un_campo_obbligatorio_nullo_e_un_testo_troppo_lungo_li_rifiuta_il_database()
    {
        var gioco = await SeedGiocoOriginaleAsync();
        await using var connessione = await ConnessioneAsync();
        var anagramma = Carica("anagramma");

        var nullo = () => Archivio.ScriviAsync(connessione, null, anagramma, SchemaTipologia.Catalogo, gioco, Json(Anagramma("x").Replace("\"esposto\":\"x\"", "\"esposto\":null", StringComparison.Ordinal)), CancellationToken.None);
        var lungo = () => Archivio.ScriviAsync(connessione, null, anagramma, SchemaTipologia.Catalogo, gioco, Json(Anagramma(new string('a', 1001))), CancellationToken.None);

        (await nullo.Should().ThrowAsync<PostgresException>()).Which.SqlState.Should().Be(PostgresErrorCodes.NotNullViolation);
        (await lungo.Should().ThrowAsync<PostgresException>()).Which.SqlState.Should().Be(PostgresErrorCodes.StringDataRightTruncation);
    }

    [Fact]
    public async Task Nello_schema_riviste_la_chiave_e_un_intero()
    {
        await using var connessione = await ConnessioneAsync();
        var anagramma = Carica("anagramma");
        // La chiave esterna verso il nucleo non è l'oggetto di questa prova: la si sospende per la sessione (la connessione è di un superutente).
        await using (var sospendi = new NpgsqlCommand("SET session_replication_role = replica", connessione))
            await sospendi.ExecuteNonQueryAsync();

        await Archivio.ScriviAsync(connessione, null, anagramma, SchemaTipologia.Riviste, 7, Json(Anagramma(Ostile)), CancellationToken.None);
        var letto = await Archivio.LeggiAsync(connessione, null, anagramma, SchemaTipologia.Riviste, 7, CancellationToken.None);

        letto!["esposto"]!.GetValue<string>().Should().Be(Ostile);
        var guidInvece = () => Archivio.LeggiAsync(connessione, null, anagramma, SchemaTipologia.Riviste, Guid.NewGuid(), CancellationToken.None);
        await guidInvece.Should().ThrowAsync<ArgumentException>();
    }
}
