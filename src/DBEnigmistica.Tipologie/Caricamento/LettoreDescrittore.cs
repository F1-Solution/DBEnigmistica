using System.Globalization;
using System.Text.Json;
using DBEnigmistica.Tipologie.Modello;

namespace DBEnigmistica.Tipologie.Caricamento;

/// <summary>Converte un descrittore già valido contro il meta-schema nei tipi del modello.</summary>
internal static class LettoreDescrittore
{
    public static Descrittore Leggi(JsonElement radice) => new(
        Id: radice.GetProperty("id").GetString()!,
        Versione: radice.GetProperty("versione").GetInt32(),
        Famiglia: Valore<FamigliaTipologia>(radice, "famiglia"),
        Nome: radice.GetProperty("nome").GetString()!,
        Descrizione: radice.GetProperty("descrizione").GetString()!,
        Regole: LeggiRegole(radice.GetProperty("regole")),
        Campi: [.. radice.GetProperty("campi").EnumerateArray().Select(LeggiCampo)],
        Elenchi: [.. radice.GetProperty("elenchi").EnumerateArray().Select(LeggiElenco)],
        Resa: new ResaDescrittore(Valore<ModelloResa>(radice.GetProperty("resa"), "modello")),
        Validatori: [.. radice.GetProperty("validatori").EnumerateArray().Select(v => Nome<NomeValidatore>(v.GetString()!))]);

    private static T Valore<T>(JsonElement oggetto, string proprieta) where T : struct, Enum => Nome<T>(oggetto.GetProperty(proprieta).GetString()!);

    private static T Nome<T>(string nome) where T : struct, Enum =>
        NomiModello.TryLeggi<T>(nome, out var valore)
            ? valore
            : throw new InvalidOperationException($"Nome '{nome}' non previsto per {typeof(T).Name}: il modello non è allineato al meta-schema.");

    private static string? Testo(JsonElement oggetto, string proprieta) =>
        oggetto.TryGetProperty(proprieta, out var valore) ? valore.GetString() : null;

    private static RegoleTipologia LeggiRegole(JsonElement regole) => new(
        regole.GetProperty("testo").GetString()!,
        regole.GetProperty("versione").GetInt32(),
        Valore<StatoRegole>(regole, "stato"),
        Testo(regole, "scritte_da"),
        Testo(regole, "approvate_il") is { } data ? DateOnly.ParseExact(data, "yyyy-MM-dd", CultureInfo.InvariantCulture) : null,
        Testo(regole, "approvate_da"));

    private static CampoDescrittore LeggiCampo(JsonElement campo) => new(
        Nome: campo.GetProperty("nome").GetString()!,
        Tipo: Valore<TipoCampo>(campo, "tipo"),
        Etichetta: Testo(campo, "etichetta"),
        Descrizione: Testo(campo, "descrizione"),
        Obbligatorio: campo.TryGetProperty("obbligatorio", out var o) && o.GetBoolean(),
        Max: campo.TryGetProperty("max", out var max) ? max.GetInt32() : null,
        Valori: campo.TryGetProperty("valori", out var valori) ? [.. valori.EnumerateArray().Select(v => v.GetString()!)] : null,
        Indice: campo.TryGetProperty("indice", out var i) && i.GetBoolean(),
        Ruolo: campo.TryGetProperty("ruolo", out var r) ? Nome<RuoloCampo>(r.GetString()!) : null);

    private static ElencoDescrittore LeggiElenco(JsonElement elenco) => new(
        elenco.GetProperty("nome").GetString()!,
        [.. elenco.GetProperty("campi").EnumerateArray().Select(LeggiCampo)],
        Testo(elenco, "etichetta"),
        Testo(elenco, "descrizione"));
}
