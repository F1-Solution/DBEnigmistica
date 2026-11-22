using DBEnigmistica.Tests.Unit.Creazione;
using DBEnigmistica.Tipologie.Caricamento;
using DBEnigmistica.Tipologie.Modello;

namespace DBEnigmistica.Tests.Unit.Tipologie;

/// <summary>Accesso ai file di <c>tipologie/</c> del repository per i test della libreria Tipologie.</summary>
internal static class PercorsiTipologie
{
    public static string Cartella => FileDelRepository.Percorso("tipologie");

    public static string Esempio(string sottocartella, string nome) => FileDelRepository.Leggi("tipologie", "_meta", "esempi", sottocartella, nome);

    public static Descrittore DescrittoreEsempio(string nome) =>
        new CaricatoreDescrittori().CaricaTesto(Esempio("descrittore", nome), nome);

    /// <summary>I due descrittori di esempio della sezione 9 di progetto-1.1.</summary>
    public static IReadOnlyList<Descrittore> DueEsempi() =>
        [DescrittoreEsempio("valido-esempio-cruciverba.v1.json"), DescrittoreEsempio("valido-esempio-anagramma.v1.json")];
}
