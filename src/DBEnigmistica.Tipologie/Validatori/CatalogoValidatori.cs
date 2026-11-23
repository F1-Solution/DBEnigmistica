using DBEnigmistica.Tipologie.Modello;

namespace DBEnigmistica.Tipologie.Validatori;

/// <summary>Tutti i validatori strutturali dell'elenco chiuso, uno per nome.</summary>
public static class CatalogoValidatori
{
    public static IReadOnlyList<IValidatoreStrutturale> Tutti { get; } =
    [
        new GrigliaCoerenteConDimensioniValidatore(),
        new NumerazioneCoerenteConGrigliaValidatore(),
        new LunghezzeCoerentiValidatore(),
        new IncrociCoerentiValidatore(),
        new DiagrammaCoerenteConSoluzioneValidatore(),
        new AnagrammaStesseLettereValidatore(),
        new CambioDiUnaLetteraValidatore(),
        new SciaradaPartiCoerentiValidatore(),
    ];
}
