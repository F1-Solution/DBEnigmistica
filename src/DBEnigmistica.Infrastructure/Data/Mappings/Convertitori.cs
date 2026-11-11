using DBEnigmistica.Domain.Diritti;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace DBEnigmistica.Infrastructure.Data.Mappings;

/// <summary>Conversioni tra gli insiemi chiusi del dominio e il testo salvato; i valori sconosciuti sono rifiutati.</summary>
internal static class Convertitori
{
    public static readonly ValueConverter<Origine, string> OrigineTesto = new(
        o => OrigineATesto(o), t => OrigineDaTesto(t));

    public static readonly ValueConverter<TipoSoggetto, string> TipoTesto = new(
        o => TipoATesto(o), t => TipoDaTesto(t));

    public static readonly ValueConverter<Territorio, string> TerritorioTesto = new(
        t => t.Codice, c => Territorio.Da(c));

    private static string OrigineATesto(Origine o) => o switch
    {
        Origine.Originale => "originale",
        Origine.Licenza => "licenza",
        Origine.PubblicoDominio => "pubblico_dominio",
        Origine.Rivista => "rivista",
        _ => throw new ArgumentOutOfRangeException(nameof(o), o, "Origine sconosciuta.")
    };

    private static Origine OrigineDaTesto(string t) => t switch
    {
        "originale" => Origine.Originale,
        "licenza" => Origine.Licenza,
        "pubblico_dominio" => Origine.PubblicoDominio,
        "rivista" => Origine.Rivista,
        _ => throw new InvalidOperationException($"Origine sconosciuta nel database: «{t}».")
    };

    private static string TipoATesto(TipoSoggetto t) => t switch
    {
        TipoSoggetto.Persona => "persona",
        TipoSoggetto.Organizzazione => "organizzazione",
        _ => throw new ArgumentOutOfRangeException(nameof(t), t, "Tipo di Soggetto sconosciuto.")
    };

    private static TipoSoggetto TipoDaTesto(string t) => t switch
    {
        "persona" => TipoSoggetto.Persona,
        "organizzazione" => TipoSoggetto.Organizzazione,
        _ => throw new InvalidOperationException($"Tipo di Soggetto sconosciuto nel database: «{t}».")
    };
}
