using DBEnigmistica.Tests.Unit.Creazione;

namespace DBEnigmistica.Tests.EndToEnd.Prototipo;

/// <summary>
/// Test nel browser: in CI (variabile CI) l'interfaccia costruita è obbligatoria e la sua assenza fa fallire il test;
/// fuori da CI, senza <c>interfaccia/dist</c>, il test viene saltato con il motivo.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class FattoConBrowserAttribute : FactAttribute
{
    public FattoConBrowserAttribute()
    {
        if (!InCi && !File.Exists(Path.Combine(CartellaInterfaccia, "index.html")))
            Skip = "Interfaccia non costruita: esegui npm run build in interfaccia/ per provare il browser.";
    }

    public static bool InCi => string.Equals(Environment.GetEnvironmentVariable("CI"), "true", StringComparison.OrdinalIgnoreCase);

    public static string CartellaInterfaccia => FileDelRepository.Percorso("interfaccia", "dist");
}
