using System.Reflection;

namespace DBEnigmistica.Tipologie.Caricamento;

/// <summary>Accesso ai file di <c>tipologie/</c> incorporati nell'assembly, con percorsi relativi a <c>tipologie/</c> e barre dritte.</summary>
public static class RisorseTipologie
{
    private const string Prefisso = "tipologie/";

    private static readonly Assembly Assembly = typeof(RisorseTipologie).Assembly;

    /// <summary>Percorsi (per esempio <c>anagramma/v1.json</c>) delle risorse con quella estensione, in ordine alfabetico.</summary>
    public static IReadOnlyList<string> Elenca(string estensione) =>
        [.. Assembly.GetManifestResourceNames()
            .Select(Normalizza)
            .Where(n => n.StartsWith(Prefisso, StringComparison.Ordinal) && n.EndsWith(estensione, StringComparison.Ordinal))
            .Select(n => n[Prefisso.Length..])
            .Order(StringComparer.Ordinal)];

    /// <summary>Legge una risorsa come testo UTF-8 con a capo LF.</summary>
    public static string Leggi(string percorso)
    {
        var nome = Assembly.GetManifestResourceNames().FirstOrDefault(n => Normalizza(n) == Prefisso + percorso)
            ?? throw new FileNotFoundException($"Risorsa delle Tipologie non trovata: {percorso}");
        using var flusso = Assembly.GetManifestResourceStream(nome)!;
        using var lettore = new StreamReader(flusso, System.Text.Encoding.UTF8);
        return lettore.ReadToEnd().Replace("\r\n", "\n", StringComparison.Ordinal);
    }

    private static string Normalizza(string nome) => nome.Replace('\\', '/');
}
