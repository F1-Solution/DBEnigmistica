using System.Text.Json;
using DBEnigmistica.Tipologie.Modello;

namespace DBEnigmistica.Tipologie.Validatori;

/// <summary>Voce dell'elenco delle definizioni; i valori mancanti o illeggibili sono null, i percorsi dei campi servono ai messaggi.</summary>
internal sealed record DefinizioneLetta(
    string Percorso,
    int? Numero,
    string? Direzione,
    string? Testo,
    string? Soluzione,
    string PercorsoTesto,
    string PercorsoSoluzione);

/// <summary>Legge l'elenco che contiene il ruolo numero, con i nomi dei campi presi dal descrittore.</summary>
internal static class DefinizioniLette
{
    public static IReadOnlyList<DefinizioneLetta> Leggi(Descrittore descrittore, JsonElement dati, out string nomeElenco)
    {
        var elenco = descrittore.ElencoConRuolo(RuoloCampo.Numero)!;
        nomeElenco = elenco.Nome;
        string? Nome(RuoloCampo ruolo) => elenco.Campi.FirstOrDefault(c => c.Ruolo == ruolo)?.Nome;
        var nomeNumero = Nome(RuoloCampo.Numero)!;
        var nomeDirezione = Nome(RuoloCampo.Direzione);
        var nomeTesto = Nome(RuoloCampo.Definizione);
        var nomeSoluzione = Nome(RuoloCampo.Soluzione);

        if (LetturaDati.Elemento(dati, elenco.Nome) is not { ValueKind: JsonValueKind.Array } voci)
            return [];

        var risultato = new List<DefinizioneLetta>();
        var i = 0;
        foreach (var voce in voci.EnumerateArray())
        {
            var percorso = $"{elenco.Nome}[{i}]";
            risultato.Add(new DefinizioneLetta(
                percorso,
                LetturaDati.Intero(voce, nomeNumero),
                nomeDirezione is null ? null : LetturaDati.Testo(voce, nomeDirezione),
                nomeTesto is null ? null : LetturaDati.Testo(voce, nomeTesto),
                nomeSoluzione is null ? null : LetturaDati.Testo(voce, nomeSoluzione),
                $"{percorso}.{nomeTesto}",
                $"{percorso}.{nomeSoluzione}"));
            i++;
        }

        return risultato;
    }
}
