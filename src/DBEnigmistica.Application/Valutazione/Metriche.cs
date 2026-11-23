using DBEnigmistica.Tipologie.Modello;

namespace DBEnigmistica.Application.Valutazione;

/// <summary>Costo e tempo di una pagina, quando noti.</summary>
public sealed record ConsumoPagina(decimal? Costo, int? DurataMs);

/// <summary>Conteggi e metriche di un gruppo di pagine; una metrica con denominatore 0 vale <c>null</c>, mai 100%.</summary>
public sealed record MetricheGruppo(
    int Pagine,
    int GiochiAttesi,
    int GiochiEstratti,
    int Abbinati,
    int Mancanti,
    int InPiu,
    double? Richiamo,
    double? Precisione,
    double? TipologiaCorretta,
    double? CaselleCorrette = null,
    double? Cer = null,
    double? CampiEsatti = null,
    double? CostoMedio = null,
    int PagineConCosto = 0,
    double? TempoMedioMs = null,
    int PagineConTempo = 0);

/// <summary>Il confronto di una pagina, da sommare nei gruppi.</summary>
public sealed record ConfrontoPagina(
    int Attesi,
    int Estratti,
    int Abbinati,
    int Mancanti,
    int InPiu,
    int CoppieSenzaChiave,
    int CoppieStessaChiave,
    DettaglioCoppia Dati,
    ConsumoPagina Consumo)
{
    public static ConfrontoPagina Calcola(
        IReadOnlyList<GiocoPagina> attesi,
        IReadOnlyList<GiocoPagina> estratti,
        IReadOnlyDictionary<string, Descrittore> descrittori,
        ConsumoPagina? consumo = null)
    {
        var conChiave = AbbinamentoGiochi.Abbina(attesi, estratti);
        var senzaChiave = AbbinamentoGiochi.Abbina(attesi, estratti, conChiave: false);

        var dati = DettaglioCoppia.Vuoto;
        foreach (var (a, e) in conChiave.Coppie)
        {
            if (attesi[a].IdTipologia is { } id && descrittori.TryGetValue(id, out var descrittore))
                dati += ConfrontoDati.Confronta(descrittore, attesi[a], estratti[e]);
        }

        return new ConfrontoPagina(
            attesi.Count, estratti.Count, conChiave.Coppie.Count, conChiave.Mancanti.Count, conChiave.InPiu.Count,
            senzaChiave.Coppie.Count,
            senzaChiave.Coppie.Count(c => attesi[c.Atteso].ChiaveTipologia == estratti[c.Estratto].ChiaveTipologia),
            dati,
            consumo ?? new ConsumoPagina(null, null));
    }
}

public static class CalcoloMetriche
{
    /// <summary>Quattro decimali, arrotondamento lontano da zero.</summary>
    public static double Arrotonda(double valore) => Math.Round(valore, 4, MidpointRounding.AwayFromZero);

    /// <summary>Micro-media: somma numeratori e denominatori delle pagine, poi arrotonda.</summary>
    public static MetricheGruppo Somma(IReadOnlyCollection<ConfrontoPagina> pagine)
    {
        var attesi = pagine.Sum(p => p.Attesi);
        var estratti = pagine.Sum(p => p.Estratti);
        var abbinati = pagine.Sum(p => p.Abbinati);
        var dati = pagine.Aggregate(DettaglioCoppia.Vuoto, (somma, p) => somma + p.Dati);
        var conCosto = pagine.Where(p => p.Consumo.Costo is not null).ToList();
        var conTempo = pagine.Where(p => p.Consumo.DurataMs is not null).ToList();
        return new MetricheGruppo(
            pagine.Count, attesi, estratti, abbinati, pagine.Sum(p => p.Mancanti), pagine.Sum(p => p.InPiu),
            Rapporto(abbinati, attesi), Rapporto(abbinati, estratti),
            Rapporto(pagine.Sum(p => p.CoppieStessaChiave), pagine.Sum(p => p.CoppieSenzaChiave)),
            Rapporto(dati.CaselleCorrette, dati.CaselleAttese),
            Rapporto(dati.CerErrori, dati.CerLunghezza),
            Rapporto(dati.CampiUguali, dati.CampiConfrontati),
            conCosto.Count == 0 ? null : Arrotonda((double)(conCosto.Sum(p => p.Consumo.Costo!.Value) / conCosto.Count)),
            conCosto.Count,
            conTempo.Count == 0 ? null : Arrotonda(conTempo.Average(p => (double)p.Consumo.DurataMs!.Value)),
            conTempo.Count);
    }

    private static double? Rapporto(int numeratore, int denominatore) =>
        denominatore == 0 ? null : Arrotonda((double)numeratore / denominatore);
}
