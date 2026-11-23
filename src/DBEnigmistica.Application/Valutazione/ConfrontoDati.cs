using System.Text.Json;
using System.Text.Json.Nodes;
using DBEnigmistica.Tipologie.Modello;

namespace DBEnigmistica.Application.Valutazione;

/// <summary>Numeratori e denominatori delle metriche sui dati di una coppia di Giochi abbinati.</summary>
public sealed record DettaglioCoppia(
    int CaselleCorrette,
    int CaselleAttese,
    int CerErrori,
    int CerLunghezza,
    int CampiUguali,
    int CampiConfrontati)
{
    public static readonly DettaglioCoppia Vuoto = new(0, 0, 0, 0, 0, 0);

    public static DettaglioCoppia operator +(DettaglioCoppia a, DettaglioCoppia b) => new(
        a.CaselleCorrette + b.CaselleCorrette, a.CaselleAttese + b.CaselleAttese, a.CerErrori + b.CerErrori,
        a.CerLunghezza + b.CerLunghezza, a.CampiUguali + b.CampiUguali, a.CampiConfrontati + b.CampiConfrontati);
}

/// <summary>Confronto dei dati di due Giochi abbinati: caselle, CER e campi esatti (progetto 1.1, sezione 4.3).</summary>
public static class ConfrontoDati
{
    private static readonly RuoloCampo[] RuoliDiTesto = [RuoloCampo.Definizione, RuoloCampo.Esposto];

    public static DettaglioCoppia Confronta(Descrittore descrittore, GiocoPagina atteso, GiocoPagina estratto)
    {
        var a = atteso.Dati ?? default;
        var e = estratto.Dati ?? default;
        var nonValutati = (atteso.CampiNonValutati ?? []).ToHashSet(StringComparer.Ordinal);
        var totale = DettaglioCoppia.Vuoto;

        if (descrittore.CampoConRuolo(RuoloCampo.Griglia) is { } griglia && !nonValutati.Contains(griglia.Nome))
            totale += Caselle(Proprieta(a, griglia.Nome), Proprieta(e, griglia.Nome));

        foreach (var campo in descrittore.Campi.Where(c => c.Ruolo is { } r && RuoliDiTesto.Contains(r) && !nonValutati.Contains(c.Nome)))
            totale += Cer(Testo(Proprieta(a, campo.Nome)), Testo(Proprieta(e, campo.Nome)));

        foreach (var elenco in descrittore.Elenchi.Where(el => !nonValutati.Contains(el.Nome) && el.Campi.Any(c => c.Ruolo is { } r && RuoliDiTesto.Contains(r))))
            totale += CerElenco(elenco, Proprieta(a, elenco.Nome), Proprieta(e, elenco.Nome));

        if (descrittore.Famiglia == FamigliaTipologia.GiocoBreve)
            totale += CampiEsatti(descrittore, a, e, nonValutati);

        return totale;
    }

    // ---- Caselle ----

    private static DettaglioCoppia Caselle(JsonElement attesa, JsonElement estratta)
    {
        if (attesa.ValueKind != JsonValueKind.Object)
            return DettaglioCoppia.Vuoto;

        var righe = Intero(attesa, "righe");
        var colonne = Intero(attesa, "colonne");
        var celleAttese = Celle(attesa);
        var numeriAttesi = Numeri(attesa);
        var celleEstratte = estratta.ValueKind == JsonValueKind.Object ? Celle(estratta) : [];
        var numeriEstratti = estratta.ValueKind == JsonValueKind.Object ? Numeri(estratta) : [];
        var stessaForma = estratta.ValueKind == JsonValueKind.Object && Intero(estratta, "righe") == righe && Intero(estratta, "colonne") == colonne;

        var contate = 0;
        var corrette = 0;
        for (var r = 1; r <= righe; r++)
        {
            for (var c = 1; c <= colonne; c++)
            {
                var casellaAttesa = Casella(celleAttese, r, c);
                if (casellaAttesa is null or '?')
                    continue;

                contate++;
                if (!stessaForma)
                    continue;

                var casellaEstratta = Casella(celleEstratte, r, c);
                if (casellaEstratta is null or '?')
                    continue;

                var stessaCategoria = (casellaAttesa == '#') == (casellaEstratta == '#');
                if (stessaCategoria && numeriAttesi.GetValueOrDefault((r, c)) == numeriEstratti.GetValueOrDefault((r, c)))
                    corrette++;
            }
        }

        return new DettaglioCoppia(corrette, contate, 0, 0, 0, 0);
    }

    private static char? Casella(IReadOnlyList<string> celle, int riga, int colonna) =>
        riga <= celle.Count && colonna <= celle[riga - 1].Length ? celle[riga - 1][colonna - 1] : null;

    private static List<string> Celle(JsonElement griglia) =>
        griglia.TryGetProperty("celle", out var celle) && celle.ValueKind == JsonValueKind.Array
            ? [.. celle.EnumerateArray().Select(v => v.ValueKind == JsonValueKind.String ? v.GetString()! : string.Empty)]
            : [];

    // Numero stampato per (riga, colonna); 0 se la casella non ha numero.
    private static Dictionary<(int Riga, int Colonna), int> Numeri(JsonElement griglia)
    {
        var numeri = new Dictionary<(int, int), int>();
        if (griglia.TryGetProperty("numeri", out var voci) && voci.ValueKind == JsonValueKind.Array)
        {
            foreach (var voce in voci.EnumerateArray())
                numeri[(Intero(voce, "riga"), Intero(voce, "colonna"))] = Intero(voce, "numero");
        }

        return numeri;
    }

    // ---- CER ----

    private static DettaglioCoppia CerElenco(ElencoDescrittore elenco, JsonElement attese, JsonElement estratte)
    {
        var campiTesto = elenco.Campi.Where(c => c.Ruolo is { } r && RuoliDiTesto.Contains(r)).ToList();
        var voceAttese = Voci(attese);
        var voceEstratte = Voci(estratte);
        var chiavi = elenco.Campi.Where(c => c.Ruolo is RuoloCampo.Numero or RuoloCampo.Direzione).Select(c => c.Nome).ToList();
        var perChiave = chiavi.Count == 2;

        var usate = new HashSet<int>();
        var totale = DettaglioCoppia.Vuoto;
        for (var i = 0; i < voceAttese.Count; i++)
        {
            var j = perChiave
                ? Enumerable.Range(0, voceEstratte.Count).FirstOrDefault(k => !usate.Contains(k) && Chiave(voceEstratte[k], chiavi) == Chiave(voceAttese[i], chiavi), -1)
                : i < voceEstratte.Count ? i : -1;
            if (j >= 0)
                usate.Add(j);

            foreach (var campo in campiTesto)
                totale += Cer(Testo(Proprieta(voceAttese[i], campo.Nome)), j >= 0 ? Testo(Proprieta(voceEstratte[j], campo.Nome)) : null);
        }

        for (var k = 0; k < voceEstratte.Count; k++)
        {
            if (perChiave ? usate.Contains(k) : k < voceAttese.Count)
                continue;

            foreach (var campo in campiTesto)
                totale += Cer(null, Testo(Proprieta(voceEstratte[k], campo.Nome)));
        }

        return totale;
    }

    private static string Chiave(JsonElement voce, List<string> nomi) =>
        string.Join('|', nomi.Select(n => Proprieta(voce, n).ToString()));

    private static List<JsonElement> Voci(JsonElement elenco) =>
        elenco.ValueKind == JsonValueKind.Array ? [.. elenco.EnumerateArray()] : [];

    // Atteso assente ed estratto assente non contano; l'atteso presente conta la sua lunghezza, l'estratto in più solo gli errori.
    private static DettaglioCoppia Cer(string? atteso, string? estratto)
    {
        if (atteso is null)
            return estratto is null ? DettaglioCoppia.Vuoto : new DettaglioCoppia(0, 0, NormalizzazioneTesto.Lunghezza(estratto), 0, 0, 0);

        return new DettaglioCoppia(0, 0, NormalizzazioneTesto.Distanza(atteso, estratto ?? string.Empty), NormalizzazioneTesto.Lunghezza(atteso), 0, 0);
    }

    // ---- Campi esatti ----

    private static DettaglioCoppia CampiEsatti(Descrittore descrittore, JsonElement attesi, JsonElement estratti, HashSet<string> nonValutati)
    {
        var confrontati = 0;
        var uguali = 0;
        foreach (var campo in descrittore.Campi.Where(c => !nonValutati.Contains(c.Nome)))
        {
            confrontati++;
            if (Uguali(Proprieta(attesi, campo.Nome), Proprieta(estratti, campo.Nome)))
                uguali++;
        }

        return new DettaglioCoppia(0, 0, 0, 0, uguali, confrontati);
    }

    private static bool Uguali(JsonElement a, JsonElement b)
    {
        var aNullo = a.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined;
        var bNullo = b.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined;
        if (aNullo || bNullo)
            return aNullo && bNullo;

        if (a.ValueKind == JsonValueKind.String && b.ValueKind == JsonValueKind.String)
            return NormalizzazioneTesto.Normalizza(a.GetString()!) == NormalizzazioneTesto.Normalizza(b.GetString()!);

        return JsonNode.DeepEquals(JsonNode.Parse(a.GetRawText()), JsonNode.Parse(b.GetRawText()));
    }

    // ---- Accesso ai dati ----

    private static JsonElement Proprieta(JsonElement oggetto, string nome) =>
        oggetto.ValueKind == JsonValueKind.Object && oggetto.TryGetProperty(nome, out var valore) ? valore : default;

    private static string? Testo(JsonElement valore) => valore.ValueKind == JsonValueKind.String ? valore.GetString() : null;

    private static int Intero(JsonElement oggetto, string nome) =>
        Proprieta(oggetto, nome) is { ValueKind: JsonValueKind.Number } n && n.TryGetInt32(out var valore) ? valore : 0;
}
