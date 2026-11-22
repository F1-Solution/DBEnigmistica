using System.Text;
using System.Text.Json;
using DBEnigmistica.Application.FornitoreAi;

namespace DBEnigmistica.Application.Valutazione;

public sealed record RisultatoBozze(IReadOnlyList<string> Scritte, IReadOnlyList<string> Saltate);

/// <summary>
/// Produce le bozze dei risultati attesi (<c>attesi/&lt;id&gt;.json</c>) da una Registrazione, controllate contro lo schema.
/// Un file esistente non si tocca, salvo una bozza con <c>sovrascrivi</c>; un file corretto mai.
/// </summary>
public sealed class GeneratoreBozzeAttese(
    RisolutorePagineSet risolutore,
    IVersionePromptEstrazione versionePrompt,
    IFabbricaArchivioRegistrazioni fabbrica,
    IInterpreteRispostaGrezza interprete,
    IValidatoreSchemaJson validatore)
{
    public async Task<RisultatoBozze> BozzaAsync(
        string cartellaSet, string fornitore, string modello, int dpi, bool sovrascrivi, CancellationToken cancellationToken = default)
    {
        var set = LettoreSet.Leggi(cartellaSet);
        var risolte = await risolutore.RisolviAsync(set, null, cancellationToken);
        var riproduzione = new FornitoreAiRiproduci(fornitore, modello, fabbrica.Apri(Path.Combine(set.Cartella, "registrazioni")), interprete, validatore);
        var schema = RisorseValutazione.SchemaRisultatoAtteso;

        var saltate = new List<string>();
        var daScrivere = new List<(string Percorso, string Id, string Testo)>();
        foreach (var risolta in risolte)
        {
            var id = risolta.Pagina.Id;
            var percorso = Path.Combine(set.Cartella, "attesi", id + ".json");
            if (File.Exists(percorso) && (!sovrascrivi || !EUnaBozza(percorso)))
            {
                saltate.Add(id);
                continue;
            }

            var risposta = await Chiedi(riproduzione, risolta, dpi, cancellationToken);
            var testo = Costruisci(risolta, risposta.Json!, fornitore, modello);
            var errori = validatore.Valida(testo, schema);
            if (errori.Count > 0)
                throw new ValutazioneException($"Pagina {id}: la bozza non rispetta lo schema del risultato atteso: {string.Join("; ", errori)}");

            daScrivere.Add((percorso, id, testo));
        }

        // Si scrive solo a lavoro finito: un errore su una pagina non lascia bozze a metà.
        foreach (var (percorso, _, testo) in daScrivere)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(percorso)!);
            await File.WriteAllTextAsync(percorso, testo, new UTF8Encoding(false), cancellationToken);
        }

        return new RisultatoBozze([.. daScrivere.Select(d => d.Id)], saltate);
    }

    private async Task<RispostaEstrazionePagina> Chiedi(IFornitoreAiEstrazione fornitore, PaginaRisolta risolta, int dpi, CancellationToken cancellationToken)
    {
        var richiesta = versionePrompt.Modello.CreaRichiesta(risolta.Impronta, [], dpi, risolta.TestoNativo);
        RispostaEstrazionePagina risposta;
        try
        {
            risposta = await fornitore.EstraiPaginaAsync(richiesta, cancellationToken);
        }
        catch (Exception ex) when (ex is RegistrazioneMancanteException or RegistrazioneNonValidaException)
        {
            throw new ValutazioneException($"Pagina {risolta.Pagina.Id}: {ex.Message}", ex);
        }

        return risposta is { Esito: EsitoEstrazione.Riuscita, Json: not null }
            ? risposta
            : throw new ValutazioneException($"Pagina {risolta.Pagina.Id}: la Registrazione non contiene una risposta valida: {risposta.Messaggio}");
    }

    // Un file illeggibile si tratta come corretto: non si sovrascrive.
    private static bool EUnaBozza(string percorso)
    {
        try
        {
            using var documento = JsonDocument.Parse(File.ReadAllText(percorso));
            return documento.RootElement.GetProperty("revisione").GetProperty("stato").GetString() == "bozza";
        }
        catch (Exception ex) when (ex is JsonException or KeyNotFoundException or InvalidOperationException or IOException)
        {
            return false;
        }
    }

    private string Costruisci(PaginaRisolta risolta, string rispostaJson, string fornitore, string modello)
    {
        using var risposta = JsonDocument.Parse(rispostaJson);
        using var memoria = new MemoryStream();
        using (var w = new Utf8JsonWriter(memoria, new JsonWriterOptions { Indented = true, NewLine = "\n" }))
        {
            w.WriteStartObject();
            w.WriteNumber("versione_formato", 1);
            w.WriteStartObject("pagina");
            w.WriteString("id", risolta.Pagina.Id);
            w.WriteString("impronta", risolta.Impronta);
            w.WriteString("origine", risolta.Pagina.Origine);
            w.WriteEndObject();
            w.WriteStartObject("revisione");
            w.WriteString("stato", "bozza");
            w.WriteStartObject("bozza_da");
            w.WriteString("fornitore", fornitore);
            w.WriteString("modello", modello);
            w.WriteString("versione_prompt", versionePrompt.VersionePrompt);
            w.WriteEndObject();
            w.WriteNull("corretto_da");
            w.WriteNull("corretto_il");
            w.WriteEndObject();
            w.WriteString("contenuto_pagina", risposta.RootElement.GetProperty("contenuto_pagina").GetString());
            w.WriteStartArray("giochi");
            var numero = 1;
            foreach (var gioco in risposta.RootElement.GetProperty("giochi").EnumerateArray())
                ScriviGioco(w, gioco, numero++);

            w.WriteEndArray();
            w.WriteNull("note");
            w.WriteEndObject();
        }

        return Encoding.UTF8.GetString(memoria.ToArray()) + "\n";
    }

    private static void ScriviGioco(Utf8JsonWriter w, JsonElement gioco, int numero)
    {
        var nota = gioco.GetProperty("stato_tipologia").GetString() == "nota";
        w.WriteStartObject();
        w.WriteString("id", $"g{numero:D2}");
        w.WriteString("stato_tipologia", nota ? "nota" : "proposta");
        if (nota)
        {
            w.WriteString("tipologia", gioco.GetProperty("tipologia").GetString());
            w.WriteNumber("versione_tipologia", gioco.GetProperty("versione_tipologia").GetInt32());
        }
        else
        {
            var famiglia = gioco.GetProperty("tipologia_proposta").GetProperty("famiglia");
            w.WritePropertyName("famiglia");
            famiglia.WriteTo(w);
        }

        w.WritePropertyName("titolo");
        gioco.GetProperty("titolo").WriteTo(w);
        w.WritePropertyName("riquadro");
        gioco.GetProperty("riquadro").WriteTo(w);
        if (nota)
        {
            w.WritePropertyName("dati");
            gioco.GetProperty("dati").WriteTo(w);
        }

        w.WriteEndObject();
    }
}
