namespace DBEnigmistica.Tests.Unit.Creazione;

/// <summary>Trova e legge i file di <c>tipologie/</c> dal repository, risalendo dal binario di test.</summary>
internal static class FileDelRepository
{
    public static string Radice()
    {
        var cartella = new DirectoryInfo(AppContext.BaseDirectory);
        while (cartella is not null && !File.Exists(Path.Combine(cartella.FullName, "Directory.Packages.props")))
            cartella = cartella.Parent;
        return cartella?.FullName ?? throw new InvalidOperationException("Radice del repository non trovata.");
    }

    public static string Percorso(params string[] parti) => Path.Combine([Radice(), .. parti]);

    public static string Leggi(params string[] parti) => File.ReadAllText(Percorso(parti)).Replace("\r\n", "\n", StringComparison.Ordinal);

    public static string Esempio(string sottocartella, string nome) => Leggi("tipologie", "_meta", "esempi", "creazione", sottocartella, nome);
}
