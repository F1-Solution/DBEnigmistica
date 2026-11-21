using System.Globalization;
using System.Text.Json;
using DBEnigmistica.Application.Creazione.Algoritmi;

namespace DBEnigmistica.Application.Creazione;

/// <summary>
/// Controlli dopo lo schema dei giochi logici (progetto 1b.1, sezione 5): indizi e segni coerenti con la soluzione,
/// soluzione valida e soluzione unica. Girano a ogni salvataggio e accettazione del revisore.
/// </summary>
public static class ControlliLogici
{
    public static IReadOnlyList<string> Verifica(string tipologia, string contenutoJson)
    {
        ArgumentNullException.ThrowIfNull(contenutoJson);
        try
        {
            using var documento = JsonDocument.Parse(contenutoJson);
            var dati = documento.RootElement.GetProperty("dati");
            return tipologia switch
            {
                "sudoku" => Sudoku(dati),
                "futoshiki" => Futoshiki(dati),
                "nonogramma" => Nonogramma(dati),
                _ => [],
            };
        }
        catch (Exception ex) when (ex is KeyNotFoundException or InvalidOperationException or FormatException or JsonException)
        {
            return ["Il contenuto non ha la forma attesa per i controlli del gioco."];
        }
    }

    private static List<string> Sudoku(JsonElement dati)
    {
        var errori = new List<string>();
        var indizi = Celle(dati, "griglia", 9, 9, errori, vuoto: '.');
        var soluzione = Celle(dati, "soluzione_griglia", 9, 9, errori, vuoto: null);
        if (indizi is null || soluzione is null)
            return errori;

        ControllaQuadrato(soluzione, 9, 3, errori);
        ControllaIndizi(indizi, soluzione, errori);
        if (errori.Count == 0)
            Unicita(VerificaUnicita.Sudoku(indizi), errori);
        return errori;
    }

    private static List<string> Futoshiki(JsonElement dati)
    {
        var errori = new List<string>();
        var n = dati.GetProperty("righe").GetInt32();
        if (n is < 4 or > 9 || dati.GetProperty("colonne").GetInt32() != n)
            return ["Il futoshiki deve essere quadrato, con lato da 4 a 9."];

        var indizi = Celle(dati, "griglia", n, n, errori, vuoto: '.');
        var soluzione = Celle(dati, "soluzione_griglia", n, n, errori, vuoto: null);
        if (indizi is null || soluzione is null)
            return errori;

        ControllaQuadrato(soluzione, n, 0, errori);
        ControllaIndizi(indizi, soluzione, errori);
        var segni = new List<SegnoFutoshiki>();
        foreach (var s in dati.GetProperty("segni").EnumerateArray())
        {
            var riga = s.GetProperty("riga").GetInt32() - 1;
            var colonna = s.GetProperty("colonna").GetInt32() - 1;
            var destra = s.GetProperty("verso").GetString() == "destra";
            var (r2, c2) = destra ? (riga, colonna + 1) : (riga + 1, colonna);
            if (riga < 0 || colonna < 0 || r2 >= n || c2 >= n)
            {
                errori.Add($"Un segno parte dalla casella ({riga + 1},{colonna + 1}) e esce dalla griglia.");
                continue;
            }

            var maggiore = s.GetProperty("segno").GetString() == "maggiore";
            var segno = maggiore ? new SegnoFutoshiki(riga, colonna, r2, c2) : new SegnoFutoshiki(r2, c2, riga, colonna);
            if (soluzione[(segno.RigaA * n) + segno.ColonnaA] <= soluzione[(segno.RigaB * n) + segno.ColonnaB])
                errori.Add($"Il segno in ({riga + 1},{colonna + 1}) contraddice la soluzione.");
            segni.Add(segno);
        }

        if (errori.Count == 0)
            Unicita(VerificaUnicita.Futoshiki(n, indizi, segni), errori);
        return errori;
    }

    private static List<string> Nonogramma(JsonElement dati)
    {
        var errori = new List<string>();
        var righe = dati.GetProperty("righe").GetInt32();
        var colonne = dati.GetProperty("colonne").GetInt32();
        var griglia = dati.GetProperty("griglia").GetProperty("celle").EnumerateArray().Select(r => r.GetString() ?? string.Empty).ToList();
        if (griglia.Any(r => r.Any(c => c != '.')))
            errori.Add("La griglia di partenza del nonogramma deve essere vuota.");

        var disegno = Disegno(dati, righe, colonne, errori);
        if (disegno is null)
            return errori;

        var attesi = NonogrammaRisolutore.CalcolaIndizi(disegno, righe, colonne);
        var indiziRighe = Indizi(dati.GetProperty("indizi_righe"), righe, "righe", errori);
        var indiziColonne = Indizi(dati.GetProperty("indizi_colonne"), colonne, "colonne", errori);
        if (indiziRighe is null || indiziColonne is null)
            return errori;

        if (!Uguali(indiziRighe, attesi.Righe))
            errori.Add("Gli indizi delle righe non corrispondono al disegno.");
        if (!Uguali(indiziColonne, attesi.Colonne))
            errori.Add("Gli indizi delle colonne non corrispondono al disegno.");
        if (errori.Count == 0)
            Unicita(VerificaUnicita.Nonogramma(righe, colonne, indiziRighe, indiziColonne), errori);
        return errori;
    }

    private static int[]? Disegno(JsonElement dati, int righe, int colonne, List<string> errori)
    {
        var celle = dati.GetProperty("soluzione_griglia").GetProperty("celle").EnumerateArray().Select(r => r.GetString() ?? string.Empty).ToList();
        if (celle.Count != righe || celle.Any(r => r.Length != colonne || r.Any(c => c is not ('#' or '.'))))
        {
            errori.Add("Il disegno del nonogramma non ha le dimensioni dichiarate o usa caratteri diversi da # e '.'.");
            return null;
        }

        return [.. celle.SelectMany(r => r.Select(c => c == '#' ? 1 : 0))];
    }

    private static List<IReadOnlyList<int>>? Indizi(JsonElement elenco, int attese, string nome, List<string> errori)
    {
        var voci = elenco.EnumerateArray().ToList();
        var risultato = new List<IReadOnlyList<int>>();
        for (var i = 0; i < voci.Count; i++)
        {
            if (voci[i].GetProperty("indice").GetInt32() != i + 1)
            {
                errori.Add($"Gli indizi delle {nome} devono essere in ordine, con indice da 1.");
                return null;
            }

            var blocchi = (voci[i].GetProperty("blocchi").GetString() ?? string.Empty)
                .Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (!blocchi.All(b => int.TryParse(b, NumberStyles.None, CultureInfo.InvariantCulture, out _)))
            {
                errori.Add($"Gli indizi delle {nome} contengono un valore che non è un numero.");
                return null;
            }

            var numeri = blocchi.Select(b => int.Parse(b, CultureInfo.InvariantCulture)).ToList();
            risultato.Add(numeri is [0] ? [] : numeri);
        }

        if (risultato.Count == attese)
            return risultato;

        errori.Add($"Servono {attese} indizi per le {nome}, ce ne sono {risultato.Count}.");
        return null;
    }

    private static bool Uguali(IReadOnlyList<IReadOnlyList<int>> a, IReadOnlyList<IReadOnlyList<int>> b) =>
        a.Count == b.Count && a.Zip(b).All(p => p.First.SequenceEqual(p.Second));

    // Cifre della griglia in ordine di riga; il vuoto (se ammesso) vale 0.
    private static int[]? Celle(JsonElement dati, string nome, int righe, int colonne, List<string> errori, char? vuoto)
    {
        var celle = dati.GetProperty(nome).GetProperty("celle").EnumerateArray().Select(r => r.GetString() ?? string.Empty).ToList();
        if (celle.Count != righe || celle.Any(r => r.Length != colonne))
        {
            errori.Add($"La griglia «{nome}» non ha le dimensioni {righe}x{colonne}.");
            return null;
        }

        var valori = new List<int>();
        foreach (var carattere in celle.SelectMany(r => r))
        {
            if (carattere == vuoto)
                valori.Add(0);
            else if (carattere >= '1' && carattere <= '0' + Math.Max(righe, colonne))
                valori.Add(carattere - '0');
            else
            {
                errori.Add($"La griglia «{nome}» contiene il carattere «{carattere}», non ammesso.");
                return null;
            }
        }

        return [.. valori];
    }

    // Quadrato latino di lato n; con riquadro > 0 anche i riquadri (sudoku).
    private static void ControllaQuadrato(int[] soluzione, int n, int riquadro, List<string> errori)
    {
        var unita = new List<IEnumerable<int>>();
        for (var i = 0; i < n; i++)
        {
            var riga = i;
            unita.Add(Enumerable.Range(0, n).Select(c => soluzione[(riga * n) + c]));
            unita.Add(Enumerable.Range(0, n).Select(r => soluzione[(r * n) + riga]));
        }

        if (riquadro > 0)
        {
            for (var br = 0; br < n; br += riquadro)
            {
                for (var bc = 0; bc < n; bc += riquadro)
                {
                    var (r0, c0) = (br, bc);
                    unita.Add(Enumerable.Range(0, n).Select(k => soluzione[((r0 + (k / riquadro)) * n) + c0 + (k % riquadro)]));
                }
            }
        }

        if (unita.Any(u => u.Distinct().Count() != n))
            errori.Add("La soluzione non è valida: una riga, una colonna o un riquadro ripete un numero.");
    }

    private static void ControllaIndizi(int[] indizi, int[] soluzione, List<string> errori)
    {
        for (var i = 0; i < indizi.Length; i++)
        {
            if (indizi[i] != 0 && indizi[i] != soluzione[i])
            {
                errori.Add("Un indizio contraddice la soluzione.");
                return;
            }
        }
    }

    private static void Unicita(EsitoUnicita esito, List<string> errori)
    {
        if (esito == EsitoUnicita.Nessuna)
            errori.Add("Gli indizi non hanno alcuna soluzione.");
        else if (esito == EsitoUnicita.Multiple)
            errori.Add("Gli indizi hanno più di una soluzione: la soluzione deve essere unica.");
    }
}
