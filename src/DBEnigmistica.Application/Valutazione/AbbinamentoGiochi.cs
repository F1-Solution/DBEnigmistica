using System.Text.Json;

namespace DBEnigmistica.Application.Valutazione;

/// <summary>Riquadro normalizzato (0-1), origine in alto a sinistra.</summary>
public sealed record RiquadroGioco(double X, double Y, double Larghezza, double Altezza);

/// <summary>
/// Un Gioco di una pagina, atteso o estratto; la chiave è <c>nota:&lt;id&gt;</c> oppure <c>proposta</c>.
/// <c>Dati</c> è l'oggetto dei campi e degli elenchi (assente per una proposta); <c>CampiNonValutati</c> vale solo per l'atteso.
/// </summary>
public sealed record GiocoPagina(
    string ChiaveTipologia,
    RiquadroGioco Riquadro,
    JsonElement? Dati = null,
    IReadOnlyList<string>? CampiNonValutati = null)
{
    private const string PrefissoNota = "nota:";

    /// <summary>Id della Tipologia nota, oppure <c>null</c> per una proposta.</summary>
    public string? IdTipologia => ChiaveTipologia.StartsWith(PrefissoNota, StringComparison.Ordinal) ? ChiaveTipologia[PrefissoNota.Length..] : null;
}

public sealed record EsitoAbbinamento(
    IReadOnlyList<(int Atteso, int Estratto)> Coppie,
    IReadOnlyList<int> Mancanti,
    IReadOnlyList<int> InPiu);

/// <summary>Abbinamento goloso e deterministico dei Giochi di una pagina (progetto 1.1, sezione 4.2).</summary>
public static class AbbinamentoGiochi
{
    public const double SogliaIou = 0.5;

    public static EsitoAbbinamento Abbina(IReadOnlyList<GiocoPagina> attesi, IReadOnlyList<GiocoPagina> estratti, bool conChiave = true)
    {
        var candidate = new List<(double Iou, int Atteso, int Estratto)>();
        for (var a = 0; a < attesi.Count; a++)
        {
            for (var e = 0; e < estratti.Count; e++)
            {
                if (conChiave && attesi[a].ChiaveTipologia != estratti[e].ChiaveTipologia)
                    continue;

                var iou = Iou(attesi[a].Riquadro, estratti[e].Riquadro);
                if (iou >= SogliaIou)
                    candidate.Add((iou, a, e));
            }
        }

        var presiAttesi = new HashSet<int>();
        var presiEstratti = new HashSet<int>();
        var coppie = new List<(int, int)>();
        foreach (var (_, atteso, estratto) in candidate.OrderByDescending(c => c.Iou).ThenBy(c => c.Atteso).ThenBy(c => c.Estratto))
        {
            if (presiAttesi.Contains(atteso) || presiEstratti.Contains(estratto))
                continue;

            presiAttesi.Add(atteso);
            presiEstratti.Add(estratto);
            coppie.Add((atteso, estratto));
        }

        coppie.Sort();
        return new EsitoAbbinamento(
            coppie,
            Enumerable.Range(0, attesi.Count).Where(i => !presiAttesi.Contains(i)).ToList(),
            Enumerable.Range(0, estratti.Count).Where(i => !presiEstratti.Contains(i)).ToList());
    }

    /// <summary>Area dell'intersezione su area dell'unione.</summary>
    public static double Iou(RiquadroGioco a, RiquadroGioco b)
    {
        var larghezza = Math.Min(a.X + a.Larghezza, b.X + b.Larghezza) - Math.Max(a.X, b.X);
        var altezza = Math.Min(a.Y + a.Altezza, b.Y + b.Altezza) - Math.Max(a.Y, b.Y);
        if (larghezza <= 0 || altezza <= 0)
            return 0;

        var intersezione = larghezza * altezza;
        var unione = a.Larghezza * a.Altezza + b.Larghezza * b.Altezza - intersezione;
        return unione <= 0 ? 0 : intersezione / unione;
    }
}
