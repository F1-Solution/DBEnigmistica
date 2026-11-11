using DBEnigmistica.Application.Registro;
using DBEnigmistica.Domain.Registro;
using DBEnigmistica.Infrastructure.Repositories;
using FluentAssertions;
using Npgsql;

namespace DBEnigmistica.Tests.Integration;

/// <summary>Registro in sola aggiunta, catena concorrente, alterazione da superutente e ruoli (criterio 9).</summary>
[Collection(PostgresCollection.Name)]
[Trait("Category", "Integration")]
public class RegistroIntegrationTests : IntegrationTestBase
{
    public RegistroIntegrationTests(PostgresFixture fixture) : base(fixture)
    {
    }

    private static NuovoEvento Evento(string entitaId, string dati = "{\"k\":1}") =>
        new(DateTime.UtcNow, "gioco_creato", "gioco_originale", entitaId, "Alessio", dati);

    private async Task<EventoRegistro> AggiungiAsync(NuovoEvento evento)
    {
        await using var context = NewContext();
        return await new RegistroEventiRepository(context).AggiungiAsync(evento);
    }

    private async Task<EsitoVerifica> VerificaAsync()
    {
        await using var context = NewContext();
        return await new VerificaIntegritaRegistro(new RegistroEventiRepository(context)).EseguiAsync();
    }

    [Fact]
    public async Task Added_events_form_a_chain_that_starts_from_zero()
    {
        var primo = await AggiungiAsync(Evento("1"));
        var secondo = await AggiungiAsync(Evento("2"));

        primo.ImprontaPrecedente.Should().Equal(new byte[32]);
        secondo.ImprontaPrecedente.Should().Equal(primo.Impronta);
        secondo.Id.Should().BeGreaterThan(primo.Id);
        (await VerificaAsync()).Integra.Should().BeTrue();
    }

    [Fact]
    public async Task Event_is_read_back_with_the_same_instant_and_data()
    {
        var scritto = await AggiungiAsync(Evento("1", "{\"titolo\":\"è\"}"));

        await using var context = NewContext();
        var letto = await new RegistroEventiRepository(context).LeggiInOrdineAsync().SingleAsync();

        letto.AvvenutoIl.Should().Be(scritto.AvvenutoIl);
        letto.AvvenutoIl.Kind.Should().Be(DateTimeKind.Utc);
        letto.Dati.Should().Be("{\"titolo\":\"è\"}");
        letto.Impronta.Should().Equal(scritto.Impronta);
    }

    [Fact]
    public async Task Update_delete_and_truncate_are_rejected()
    {
        await AggiungiAsync(Evento("1"));

        (await SqlStateAsync("UPDATE registro.eventi SET autore = 'Intruso'")).Should().Be(PostgresErrorCodes.InsufficientPrivilege);
        (await SqlStateAsync("DELETE FROM registro.eventi")).Should().Be(PostgresErrorCodes.InsufficientPrivilege);
        (await SqlStateAsync("TRUNCATE registro.eventi")).Should().Be(PostgresErrorCodes.InsufficientPrivilege);
        (await ScalarAsync<long>("SELECT count(*) FROM registro.eventi")).Should().Be(1);
    }

    [Fact]
    public async Task Concurrent_additions_keep_the_chain_linear()
    {
        var compiti = Enumerable.Range(0, 8).Select(async t =>
        {
            for (var i = 0; i < 5; i++)
                await AggiungiAsync(Evento($"{t}-{i}"));
        });

        await Task.WhenAll(compiti);

        var esito = await VerificaAsync();
        esito.Integra.Should().BeTrue(esito.Motivo);
        esito.EventiVerificati.Should().Be(40);
        (await ScalarAsync<long>("SELECT count(DISTINCT impronta_precedente) FROM registro.eventi")).Should().Be(40);
    }

    [Fact]
    public async Task Fork_of_the_chain_is_rejected_by_the_trigger_and_by_the_unique_constraint_without_triggers()
    {
        var primo = await AggiungiAsync(Evento("1"));
        var altro = Evento("2");
        var parametri = new[]
        {
            P("p", primo.ImprontaPrecedente), P("h", ImprontaEvento.Calcola(primo.ImprontaPrecedente, altro))
        };
        const string insert =
            "INSERT INTO registro.eventi (avvenuto_il, tipo, entita, entita_id, autore, dati, impronta_precedente, impronta) " +
            "VALUES (now(), 'x', 'y', '1', 'a', '{}', @p, @h)";

        (await SqlStateAsync(insert, parameters: parametri)).Should().Be(PostgresErrorCodes.CheckViolation);

        // Senza trigger (superutente in modalità replica) resta il vincolo UNIQUE come ultima difesa.
        var senzaTrigger = await SqlStateAsync("SET session_replication_role = replica; " + insert,
            parameters: [P("p", primo.ImprontaPrecedente), P("h", ImprontaEvento.Calcola(primo.ImprontaPrecedente, altro))]);
        senzaTrigger.Should().Be(PostgresErrorCodes.UniqueViolation);
    }

    [Fact]
    public async Task Alteration_by_a_superuser_is_detected_with_the_right_row_id()
    {
        await AggiungiAsync(Evento("1"));
        var alterato = await AggiungiAsync(Evento("2"));
        await AggiungiAsync(Evento("3"));

        await using (var connection = new NpgsqlConnection(ConnectionString))
        {
            await connection.OpenAsync();
            await using var command = new NpgsqlCommand(
                "SET session_replication_role = replica; " +
                "UPDATE registro.eventi SET dati = '{\"k\":999}' WHERE id = @id", connection);
            command.Parameters.AddWithValue("id", alterato.Id);
            await command.ExecuteNonQueryAsync();
        }

        var esito = await VerificaAsync();

        esito.Integra.Should().BeFalse();
        esito.PrimoIdNonValido.Should().Be(alterato.Id);
        esito.EventiVerificati.Should().Be(1);
    }

    private const string InsertGrezzo =
        "INSERT INTO registro.eventi (avvenuto_il, tipo, entita, entita_id, autore, dati, impronta_precedente, impronta) " +
        "VALUES (now(), 'x', 'y', @eid, @autore, '{}', @p, @h)";

    [Theory]
    [InlineData("", "autore")]
    [InlineData("   ", "autore")]
    [InlineData("1", "\t")]
    [InlineData("1", " \n ")]
    public async Task Rows_with_a_blank_entity_id_or_author_are_rejected_by_the_database(string entitaId, string autore)
    {
        var stato = await SqlStateAsync(InsertGrezzo,
            parameters: [P("eid", entitaId), P("autore", autore), P("p", new byte[32]), P("h", new byte[32])]);

        stato.Should().Be(PostgresErrorCodes.CheckViolation);
    }

    [Fact]
    public async Task Row_passing_the_checks_but_not_domain_validation_is_reported_by_the_verification_not_thrown()
    {
        // PostgreSQL accetta un JSON annidato oltre i 64 livelli che il dominio rifiuta.
        var profondo = string.Concat(Enumerable.Repeat("{\"a\":", 70)) + "1" + new string('}', 70);
        var stato = await SqlStateAsync(
            "INSERT INTO registro.eventi (avvenuto_il, tipo, entita, entita_id, autore, dati, impronta_precedente, impronta) " +
            "VALUES (now(), 'x', 'y', '1', 'a', @dati, @p, @h)",
            parameters: [P("dati", profondo), P("p", new byte[32]), P("h", new byte[32])]);
        stato.Should().BeNull("il test presuppone che i CHECK del database lascino passare il JSON profondo");

        var esito = await VerificaAsync();

        esito.Integra.Should().BeFalse();
        esito.PrimoIdNonValido.Should().BePositive();
        esito.Motivo.Should().Be(VerificaCatena.MotivoRigaNonValida);
    }

    [Fact]
    public async Task Raw_insert_with_a_wrong_previous_hash_is_rejected_on_an_empty_table()
    {
        var stato = await SqlStateAsync(InsertGrezzo,
            parameters: [P("eid", "1"), P("autore", "a"), P("p", new byte[32].Select(_ => (byte)7).ToArray()), P("h", new byte[32])]);

        stato.Should().Be(PostgresErrorCodes.CheckViolation);
    }

    [Fact]
    public async Task Raw_insert_that_does_not_continue_the_chain_is_rejected_for_any_role_with_insert()
    {
        var primo = await AggiungiAsync(Evento("1"));

        foreach (var ruolo in new[] { null, "dbe_estrazione", "dbe_creazione", "dbe_consegna" })
        {
            var stato = await SqlStateAsync(InsertGrezzo, ruolo,
                P("eid", "2"), P("autore", "a"), P("p", new byte[32]), P("h", new byte[32]));
            stato.Should().Be(PostgresErrorCodes.CheckViolation, ruolo ?? "superutente");
        }

        (await ScalarAsync<long>("SELECT count(*) FROM registro.eventi")).Should().Be(1);
        (await VerificaAsync()).Integra.Should().BeTrue();
        primo.Id.Should().BePositive();
    }

    [Fact]
    public async Task Raw_insert_with_a_hand_picked_id_is_rejected()
    {
        var primo = await AggiungiAsync(Evento("1"));
        var secondo = Evento("2");

        var stato = await SqlStateAsync(
            "INSERT INTO registro.eventi (id, avvenuto_il, tipo, entita, entita_id, autore, dati, impronta_precedente, impronta) " +
            "OVERRIDING SYSTEM VALUE VALUES (@id, @t, @tipo, @ent, @eid, @autore, @dati, @p, @h)",
            parameters:
            [
                P("id", 1_000_000L), P("t", secondo.AvvenutoIl), P("tipo", secondo.Tipo), P("ent", secondo.Entita),
                P("eid", secondo.EntitaId), P("autore", secondo.Autore), P("dati", secondo.Dati),
                P("p", primo.Impronta), P("h", ImprontaEvento.Calcola(primo.Impronta, secondo))
            ]);

        stato.Should().Be(PostgresErrorCodes.CheckViolation);
        (await ScalarAsync<long>("SELECT count(*) FROM registro.eventi")).Should().Be(1);
    }

    [Fact]
    public async Task Raw_insert_that_continues_the_chain_is_accepted()
    {
        var primo = await AggiungiAsync(Evento("1"));
        var secondo = Evento("2");

        var stato = await SqlStateAsync(
            "INSERT INTO registro.eventi (avvenuto_il, tipo, entita, entita_id, autore, dati, impronta_precedente, impronta) " +
            "VALUES (@t, @tipo, @ent, @eid, @autore, @dati, @p, @h)",
            parameters:
            [
                P("t", secondo.AvvenutoIl), P("tipo", secondo.Tipo), P("ent", secondo.Entita),
                P("eid", secondo.EntitaId), P("autore", secondo.Autore), P("dati", secondo.Dati),
                P("p", primo.Impronta), P("h", ImprontaEvento.Calcola(primo.Impronta, secondo))
            ]);

        stato.Should().BeNull();
        (await VerificaAsync()).EventiVerificati.Should().Be(2);
    }

    [Fact]
    public async Task Failed_addition_leaves_nothing_tracked_and_a_later_save_does_not_reinsert_the_row()
    {
        await using var context = NewContext();
        var repository = new RegistroEventiRepository(context);
        // Un NUL passa la validazione del dominio ma PostgreSQL lo rifiuta nel testo.
        var guasto = new NuovoEvento(DateTime.UtcNow, "gioco_creato", "gioco_originale", "1", "a b", "{}");

        var aggiunta = async () => await repository.AggiungiAsync(guasto);
        await aggiunta.Should().ThrowAsync<Exception>();

        context.ChangeTracker.Entries().Should().BeEmpty();
        await context.SaveChangesAsync();
        (await ScalarAsync<long>("SELECT count(*) FROM registro.eventi")).Should().Be(0);

        var riuscito = await repository.AggiungiAsync(Evento("2"));
        riuscito.ImprontaPrecedente.Should().Equal(new byte[32]);
        (await VerificaAsync()).EventiVerificati.Should().Be(1);
    }

    [Fact]
    public async Task Extraction_role_cannot_update_the_registry()
    {
        await AggiungiAsync(Evento("1"));

        (await SqlStateAsync("UPDATE registro.eventi SET autore = 'x'", role: "dbe_estrazione"))
            .Should().Be(PostgresErrorCodes.InsufficientPrivilege);
        (await SqlStateAsync("DELETE FROM registro.eventi", role: "dbe_estrazione"))
            .Should().Be(PostgresErrorCodes.InsufficientPrivilege);
    }

    [Fact]
    public async Task Creation_role_cannot_read_the_magazine_schema_at_all()
    {
        (await SqlStateAsync("SELECT 1 FROM riviste.games", role: "dbe_creazione"))
            .Should().Be(PostgresErrorCodes.InsufficientPrivilege);
        (await SqlStateAsync("SELECT 1 FROM riviste.magazines", role: "dbe_creazione"))
            .Should().Be(PostgresErrorCodes.InsufficientPrivilege);
    }

    [Fact]
    public async Task Roles_have_the_documented_privileges()
    {
        // Estrazione scrive nelle Riviste e aggiunge al registro, ma non vede il catalogo.
        (await SqlStateAsync("SELECT 1 FROM riviste.games", role: "dbe_estrazione")).Should().BeNull();
        (await SqlStateAsync("SELECT 1 FROM catalogo.giochi", role: "dbe_estrazione")).Should().Be(PostgresErrorCodes.InsufficientPrivilege);
        (await SqlStateAsync("SELECT 1 FROM registro.eventi", role: "dbe_estrazione")).Should().BeNull();

        // Creazione scrive nel catalogo e legge le Tipologie.
        (await SqlStateAsync("SELECT 1 FROM catalogo.giochi", role: "dbe_creazione")).Should().BeNull();
        (await SqlStateAsync("SELECT 1 FROM tipologie.game_type_schemas", role: "dbe_creazione")).Should().BeNull();

        // Originalità legge Riviste e catalogo ma non tocca il registro né scrive.
        (await SqlStateAsync("SELECT 1 FROM riviste.games", role: "dbe_originalita")).Should().BeNull();
        (await SqlStateAsync("SELECT 1 FROM catalogo.giochi", role: "dbe_originalita")).Should().BeNull();
        (await SqlStateAsync("SELECT 1 FROM registro.eventi", role: "dbe_originalita")).Should().Be(PostgresErrorCodes.InsufficientPrivilege);
        (await SqlStateAsync("DELETE FROM catalogo.giochi", role: "dbe_originalita")).Should().Be(PostgresErrorCodes.InsufficientPrivilege);

        // Consegna legge il catalogo, non le Riviste.
        (await SqlStateAsync("SELECT 1 FROM catalogo.giochi", role: "dbe_consegna")).Should().BeNull();
        (await SqlStateAsync("SELECT 1 FROM riviste.games", role: "dbe_consegna")).Should().Be(PostgresErrorCodes.InsufficientPrivilege);
    }

    [Fact]
    public async Task Public_is_locked_out_of_every_context_schema()
    {
        var utenti = await ScalarAsync<long>(
            "SELECT count(*) FROM pg_namespace n, aclexplode(n.nspacl) a " +
            "WHERE n.nspname IN ('riviste','catalogo','tipologie','registro') AND a.grantee = 0");

        utenti.Should().Be(0);
    }
}
