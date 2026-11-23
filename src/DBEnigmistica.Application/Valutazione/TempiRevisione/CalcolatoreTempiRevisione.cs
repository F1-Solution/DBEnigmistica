using DBEnigmistica.Application.Creazione;

namespace DBEnigmistica.Application.Valutazione.TempiRevisione;

/// <summary>Calcola il rapporto sui candidati: funzione pura, due esecuzioni sugli stessi dati danno lo stesso rapporto salvo la data.</summary>
public static class CalcolatoreTempiRevisione
{
    /// <summary>Sotto questo numero di rivisti la misura è solo indicativa.</summary>
    public const int RivistiPerMisuraAttendibile = 10;

    private sealed record Definizione(string Nome, string[] Tipologie, int? MinutiMassimi);

    // Ordine fisso dei gruppi della prova e soglie di G1 (sezione 8.2).
    private static readonly Definizione[] Definizioni =
    [
        new("crittografia_breve", ["crittografia-breve"], 20),
        new("anagramma", ["anagramma"], 20),
        new("cruciverba", ["cruciverba-classico"], 60),
        new("rebus", ["rebus"], null),
        new("sudoku", ["sudoku"], null),
        new("giochi_logici", ["futoshiki", "nonogramma"], null),
    ];

    public static RapportoTempiRevisione Calcola(IEnumerable<CandidatoPerRapporto> candidati, DateTimeOffset generatoIl)
    {
        ArgumentNullException.ThrowIfNull(candidati);
        var elenco = candidati.ToList();
        return new RapportoTempiRevisione(generatoIl, [.. Definizioni.Select(d => Gruppo(d, elenco))]);
    }

    private static GruppoRapporto Gruppo(Definizione d, List<CandidatoPerRapporto> tutti)
    {
        var del = tutti.Where(c => d.Tipologie.Contains(c.Tipologia)).ToList();
        var validi = del.Count(c => StatoCandidato.Valido(c.Stato));
        var nonValidi = del.Count(c => c.Stato == StatoCandidato.NonValidoSchema);
        var errori = del.Count(c => c.Stato == StatoCandidato.ErroreGenerazione);
        var rivisti = del.Where(c => c.Stato is StatoCandidato.Accettato or StatoCandidato.Scartato).ToList();
        var accettati = rivisti.Count(c => c.Stato == StatoCandidato.Accettato);
        var minuti = rivisti.Select(c => c.TempoAttivoMs / 60_000.0).ToList();
        var generati = validi + nonValidi;

        // La soglia si confronta con la mediana come è mostrata (4 decimali).
        var mediana = Arrotonda(Quantili.Calcola(minuti, 0.5));
        double? quotaAccettata = rivisti.Count == 0 ? null : (double)accettati / rivisti.Count;
        var soglia = d.MinutiMassimi is { } massimo
            ? new SogliaG1(massimo, 0.5, mediana is null ? null : mediana <= massimo, quotaAccettata is null ? null : quotaAccettata >= 0.5)
            : null;

        return new GruppoRapporto(
            d.Nome,
            [.. d.Tipologie.Where(t => del.Any(c => c.Tipologia == t)).Select(t =>
                new TipologiaNelRapporto(t, [.. del.Where(c => c.Tipologia == t).Select(c => c.Versione).Distinct().Order()]))],
            generati,
            validi,
            nonValidi,
            errori,
            generati == 0 ? null : Arrotonda((double)nonValidi / generati),
            rivisti.Count,
            validi - rivisti.Count,
            accettati,
            rivisti.Count - accettati,
            Arrotonda(quotaAccettata),
            new MinutiRapporto(
                Arrotonda(mediana),
                Arrotonda(Quantili.Calcola(minuti, 0.25)),
                Arrotonda(Quantili.Calcola(minuti, 0.75)),
                minuti.Count == 0 ? null : Arrotonda(minuti.Min()),
                minuti.Count == 0 ? null : Arrotonda(minuti.Max())),
            rivisti.Count < RivistiPerMisuraAttendibile,
            soglia);
    }

    private static double? Arrotonda(double? valore) => valore is { } v ? CalcoloMetriche.Arrotonda(v) : null;
}
