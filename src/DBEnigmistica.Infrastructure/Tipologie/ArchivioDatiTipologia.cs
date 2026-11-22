using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using DBEnigmistica.Tipologie.Generatore;
using DBEnigmistica.Tipologie.Modello;
using Npgsql;
using NpgsqlTypes;

namespace DBEnigmistica.Infrastructure.Tipologie;

/// <summary>Il valore di un campo non è del tipo previsto dal descrittore.</summary>
public sealed class ValoreTipologiaNonValidoException(string percorso, string motivo)
    : Exception($"Valore non valido in '{percorso}': {motivo}")
{
    public string Percorso { get; } = percorso;
}

/// <summary>
/// Scrive e legge i dati di un Gioco nelle tabelle della sua Tipologia con comandi parametrizzati.
/// Dal descrittore (già validato) vengono solo gli identificatori, sempre quotati; ogni valore viaggia come parametro.
/// </summary>
public sealed class ArchivioDatiTipologia
{
    /// <summary>Scrive i dati del Gioco (<paramref name="giocoId"/>: <see cref="int"/> per riviste, <see cref="Guid"/> per catalogo). Senza transazione ne usa una propria.</summary>
    public async Task ScriviAsync(
        NpgsqlConnection connessione, NpgsqlTransaction? transazione, Descrittore descrittore, SchemaTipologia schema,
        object giocoId, JsonElement dati, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(connessione);
        ArgumentNullException.ThrowIfNull(descrittore);
        ControllaChiave(schema, giocoId);
        if (dati.ValueKind != JsonValueKind.Object)
            throw new ValoreTipologiaNonValidoException("dati", "serve un oggetto.");

        var proprie = transazione is null;
        var tx = transazione ?? await connessione.BeginTransactionAsync(cancellationToken);
        try
        {
            ControllaChiaviEstranee(descrittore, dati);
            await InserisciAsync(connessione, tx, NomiSql.Qualificato(schema, NomiSql.Tabella(descrittore)), null, descrittore.Campi, dati, giocoId, cancellationToken);
            foreach (var elenco in descrittore.Elenchi)
                await ScriviElencoAsync(connessione, tx, descrittore, elenco, schema, giocoId, dati, cancellationToken);

            if (proprie)
                await tx.CommitAsync(cancellationToken);
        }
        finally
        {
            if (proprie)
                await tx.DisposeAsync();
        }
    }

    /// <summary>Legge i dati del Gioco come oggetto con le chiavi dei campi e degli elenchi; null se il Gioco non ha dati.</summary>
    public async Task<JsonObject?> LeggiAsync(
        NpgsqlConnection connessione, NpgsqlTransaction? transazione, Descrittore descrittore, SchemaTipologia schema,
        object giocoId, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(connessione);
        ArgumentNullException.ThrowIfNull(descrittore);
        ControllaChiave(schema, giocoId);

        var risultato = new JsonObject();
        var righe = await LeggiRigheAsync(connessione, transazione, NomiSql.Qualificato(schema, NomiSql.Tabella(descrittore)), descrittore.Campi, giocoId, false, cancellationToken);
        if (righe.Count == 0)
            return null;

        foreach (var campo in descrittore.Campi)
            risultato[campo.Nome] = righe[0][campo.Nome]?.DeepClone();

        foreach (var elenco in descrittore.Elenchi)
        {
            var voci = await LeggiRigheAsync(connessione, transazione, NomiSql.Qualificato(schema, NomiSql.Tabella(descrittore, elenco)), elenco.Campi, giocoId, true, cancellationToken);
            risultato[elenco.Nome] = new JsonArray([.. voci.Select(v => (JsonNode)v)]);
        }

        return risultato;
    }

    private static async Task ScriviElencoAsync(
        NpgsqlConnection connessione, NpgsqlTransaction tx, Descrittore descrittore, ElencoDescrittore elenco, SchemaTipologia schema,
        object giocoId, JsonElement dati, CancellationToken cancellationToken)
    {
        if (!dati.TryGetProperty(elenco.Nome, out var voci) || voci.ValueKind != JsonValueKind.Array)
            throw new ValoreTipologiaNonValidoException(elenco.Nome, "serve un elenco.");

        var tabella = NomiSql.Qualificato(schema, NomiSql.Tabella(descrittore, elenco));
        var posizione = 0;
        foreach (var voce in voci.EnumerateArray())
        {
            if (voce.ValueKind != JsonValueKind.Object)
                throw new ValoreTipologiaNonValidoException($"{elenco.Nome}[{posizione}]", "serve un oggetto.");

            await InserisciAsync(connessione, tx, tabella, posizione, elenco.Campi, voce, giocoId, cancellationToken, $"{elenco.Nome}[{posizione}].");
            posizione++;
        }
    }

    private static async Task InserisciAsync(
        NpgsqlConnection connessione, NpgsqlTransaction tx, string tabella, int? posizione, IReadOnlyList<CampoDescrittore> campi,
        JsonElement valori, object giocoId, CancellationToken cancellationToken, string prefisso = "")
    {
        var colonne = new List<string> { NomiSql.Quota("gioco_id") };
        var segnaposti = new List<string> { "@gioco_id" };
        await using var comando = new NpgsqlCommand { Connection = connessione, Transaction = tx };
        comando.Parameters.Add(new NpgsqlParameter("gioco_id", giocoId));

        if (posizione is not null)
        {
            colonne.Add(NomiSql.Quota("posizione"));
            segnaposti.Add("@posizione");
            comando.Parameters.Add(new NpgsqlParameter("posizione", posizione.Value));
        }

        for (var i = 0; i < campi.Count; i++)
        {
            var campo = campi[i];
            colonne.Add(NomiSql.Quota(campo.Nome));
            segnaposti.Add($"@p{i}");
            valori.TryGetProperty(campo.Nome, out var valore);
            comando.Parameters.Add(Parametro($"p{i}", campo, valore, prefisso + campo.Nome));
        }

        comando.CommandText = $"INSERT INTO {tabella} ({string.Join(", ", colonne)}) VALUES ({string.Join(", ", segnaposti)})";
        await comando.ExecuteNonQueryAsync(cancellationToken);
    }

    // Un campo assente o null vale NULL; il database rifiuta il NULL di un campo obbligatorio.
    private static NpgsqlParameter Parametro(string nome, CampoDescrittore campo, JsonElement valore, string percorso)
    {
        if (valore.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null)
            return new NpgsqlParameter(nome, DBNull.Value) { NpgsqlDbType = TipoNpgsql(campo.Tipo) };

        return campo.Tipo switch
        {
            TipoCampo.Testo or TipoCampo.TestoLungo or TipoCampo.Enumerazione =>
                new NpgsqlParameter(nome, NpgsqlDbType.Text) { Value = Stringa(valore, percorso) },
            TipoCampo.Intero => new NpgsqlParameter(nome, NpgsqlDbType.Integer) { Value = Intero(valore, percorso) },
            TipoCampo.Decimale => new NpgsqlParameter(nome, NpgsqlDbType.Numeric) { Value = Decimale(valore, percorso) },
            TipoCampo.Booleano => new NpgsqlParameter(nome, NpgsqlDbType.Boolean) { Value = Booleano(valore, percorso) },
            TipoCampo.Data => new NpgsqlParameter(nome, NpgsqlDbType.TimestampTz) { Value = Data(valore, percorso) },
            TipoCampo.Griglia => new NpgsqlParameter(nome, NpgsqlDbType.Jsonb) { Value = Griglia(valore, percorso) },
            TipoCampo.Immagine => new NpgsqlParameter(nome, NpgsqlDbType.Uuid) { Value = Identificativo(valore, percorso) },
            _ => throw new ValoreTipologiaNonValidoException(percorso, "tipo di campo non previsto."),
        };
    }

    private static NpgsqlDbType TipoNpgsql(TipoCampo tipo) => tipo switch
    {
        TipoCampo.Intero => NpgsqlDbType.Integer,
        TipoCampo.Decimale => NpgsqlDbType.Numeric,
        TipoCampo.Booleano => NpgsqlDbType.Boolean,
        TipoCampo.Data => NpgsqlDbType.TimestampTz,
        TipoCampo.Griglia => NpgsqlDbType.Jsonb,
        TipoCampo.Immagine => NpgsqlDbType.Uuid,
        _ => NpgsqlDbType.Text,
    };

    private static string Stringa(JsonElement v, string p) =>
        v.ValueKind == JsonValueKind.String ? v.GetString()! : throw new ValoreTipologiaNonValidoException(p, "serve un testo.");

    private static int Intero(JsonElement v, string p) =>
        v.ValueKind == JsonValueKind.Number && v.TryGetInt32(out var n) ? n : throw new ValoreTipologiaNonValidoException(p, "serve un numero intero.");

    private static decimal Decimale(JsonElement v, string p) =>
        v.ValueKind == JsonValueKind.Number && v.TryGetDecimal(out var n) ? n : throw new ValoreTipologiaNonValidoException(p, "serve un numero.");

    private static bool Booleano(JsonElement v, string p) => v.ValueKind switch
    {
        JsonValueKind.True => true,
        JsonValueKind.False => false,
        _ => throw new ValoreTipologiaNonValidoException(p, "serve vero o falso."),
    };

    // Una data AAAA-MM-GG è la mezzanotte UTC di quel giorno.
    private static DateTimeOffset Data(JsonElement v, string p) =>
        v.ValueKind == JsonValueKind.String
        && DateOnly.TryParseExact(v.GetString(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var giorno)
            ? new DateTimeOffset(giorno.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero)
            : throw new ValoreTipologiaNonValidoException(p, "serve una data AAAA-MM-GG.");

    private static string Griglia(JsonElement v, string p) =>
        v.ValueKind == JsonValueKind.Object ? v.GetRawText() : throw new ValoreTipologiaNonValidoException(p, "serve un oggetto griglia.");

    // I ritagli non esistono ancora: in fase 1 un'immagine è un identificativo o NULL.
    private static Guid Identificativo(JsonElement v, string p) =>
        v.ValueKind == JsonValueKind.String && Guid.TryParse(v.GetString(), out var id)
            ? id
            : throw new ValoreTipologiaNonValidoException(p, "i ritagli non sono ancora disponibili: serve un identificativo.");

    private static void ControllaChiaviEstranee(Descrittore d, JsonElement dati)
    {
        var note = d.Campi.Select(c => c.Nome).Concat(d.Elenchi.Select(e => e.Nome)).ToHashSet(StringComparer.Ordinal);
        foreach (var proprieta in dati.EnumerateObject().Where(p => !note.Contains(p.Name)))
            throw new ValoreTipologiaNonValidoException(proprieta.Name, "chiave non prevista dal descrittore.");
    }

    private static void ControllaChiave(SchemaTipologia schema, object giocoId)
    {
        var valido = schema == SchemaTipologia.Riviste ? giocoId is int : giocoId is Guid;
        if (!valido)
            throw new ArgumentException($"L'id del Gioco per lo schema {NomiSql.Schema(schema)} deve essere {(schema == SchemaTipologia.Riviste ? "int" : "Guid")}.", nameof(giocoId));
    }

    private static async Task<List<JsonObject>> LeggiRigheAsync(
        NpgsqlConnection connessione, NpgsqlTransaction? tx, string tabella, IReadOnlyList<CampoDescrittore> campi,
        object giocoId, bool conPosizione, CancellationToken cancellationToken)
    {
        var colonne = string.Join(", ", campi.Select(c => NomiSql.Quota(c.Nome)));
        var ordine = conPosizione ? $" ORDER BY {NomiSql.Quota("posizione")}" : string.Empty;
        var sql = new StringBuilder($"SELECT {colonne} FROM {tabella} WHERE {NomiSql.Quota("gioco_id")} = @gioco_id{ordine}");

        await using var comando = new NpgsqlCommand(sql.ToString(), connessione, tx);
        comando.Parameters.Add(new NpgsqlParameter("gioco_id", giocoId));
        var righe = new List<JsonObject>();
        await using var lettore = await comando.ExecuteReaderAsync(cancellationToken);
        while (await lettore.ReadAsync(cancellationToken))
        {
            var riga = new JsonObject();
            for (var i = 0; i < campi.Count; i++)
                riga[campi[i].Nome] = lettore.IsDBNull(i) ? null : Valore(campi[i].Tipo, lettore, i);
            righe.Add(riga);
        }

        return righe;
    }

    private static JsonNode Valore(TipoCampo tipo, NpgsqlDataReader lettore, int i) => tipo switch
    {
        TipoCampo.Intero => JsonValue.Create(lettore.GetInt32(i)),
        TipoCampo.Decimale => JsonValue.Create(lettore.GetDecimal(i)),
        TipoCampo.Booleano => JsonValue.Create(lettore.GetBoolean(i)),
        TipoCampo.Data => JsonValue.Create(lettore.GetFieldValue<DateTimeOffset>(i).UtcDateTime.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)),
        TipoCampo.Griglia => JsonNode.Parse(lettore.GetString(i))!,
        TipoCampo.Immagine => JsonValue.Create(lettore.GetGuid(i).ToString()),
        _ => JsonValue.Create(lettore.GetString(i)),
    };
}
