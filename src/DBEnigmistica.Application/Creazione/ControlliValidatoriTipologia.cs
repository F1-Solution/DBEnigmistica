using System.Text.Json;
using DBEnigmistica.Tipologie.Caricamento;
using DBEnigmistica.Tipologie.Modello;
using DBEnigmistica.Tipologie.Validatori;

namespace DBEnigmistica.Application.Creazione;

/// <summary>Valore calcolato una volta sola, ma un errore non resta in memoria: la lettura successiva riprova.</summary>
public sealed class ValoreRiprovabile<T>(Func<T> crea)
{
    private readonly object _blocco = new();
    private T? _valore;
    private bool _pronto;

    public T Valore
    {
        get
        {
            lock (_blocco)
            {
                if (!_pronto)
                {
                    _valore = crea();
                    _pronto = true;
                }

                return _valore!;
            }
        }
    }
}

/// <summary>Esegue i validatori strutturali della 1.2 elencati nel descrittore; i problemi sono solo informazioni per il revisore.</summary>
public static class ControlliValidatoriTipologia
{
    private static readonly ValoreRiprovabile<CatalogoTipologie> Catalogo = new(() => new CaricatoreDescrittori().CaricaRisorse());

    /// <summary>Problemi trovati nel contenuto (involucro di creazione); elenco vuoto se nessuno o se la Tipologia non è nota.</summary>
    public static IReadOnlyList<string> Verifica(string tipologia, int versione, string contenutoJson) =>
        Verifica(tipologia, versione, contenutoJson, Catalogo, CatalogoValidatori.Tutti);

    /// <summary>Come sopra con i validatori indicati (per le prove).</summary>
    public static IReadOnlyList<string> Verifica(string tipologia, int versione, string contenutoJson, IReadOnlyList<IValidatoreStrutturale> validatori) =>
        Verifica(tipologia, versione, contenutoJson, Catalogo, validatori);

    /// <summary>Come sopra con il catalogo indicato (per le prove).</summary>
    public static IReadOnlyList<string> Verifica(string tipologia, int versione, string contenutoJson, ValoreRiprovabile<CatalogoTipologie> catalogo) =>
        Verifica(tipologia, versione, contenutoJson, catalogo, CatalogoValidatori.Tutti);

    // Un controllo leggero non deve mai far fallire la lettura del candidato: ogni errore, tranne l'annullamento, diventa un messaggio.
    private static IReadOnlyList<string> Verifica(
        string tipologia, int versione, string contenutoJson, ValoreRiprovabile<CatalogoTipologie> catalogo, IReadOnlyList<IValidatoreStrutturale> validatori)
    {
        ArgumentNullException.ThrowIfNull(contenutoJson);
        try
        {
            var descrittore = catalogo.Valore.Descrittori.FirstOrDefault(d => d.Id == tipologia && d.Versione == versione);
            if (descrittore is null || descrittore.Validatori.Count == 0)
                return [];

            using var documento = JsonDocument.Parse(contenutoJson);
            if (!documento.RootElement.TryGetProperty("dati", out var dati))
                return ["Il contenuto non ha la parte «dati»: i controlli di coerenza non si possono eseguire."];

            return Esegui(descrittore, dati, validatori);
        }
        catch (JsonException)
        {
            return ["Il contenuto non è un JSON leggibile: i controlli di coerenza non si possono eseguire."];
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return ["I controlli di coerenza non si possono eseguire su questo contenuto."];
        }
    }

    private static List<string> Esegui(Descrittore descrittore, JsonElement dati, IReadOnlyList<IValidatoreStrutturale> validatori)
    {
        var messaggi = new List<string>();
        foreach (var nome in descrittore.Validatori)
        {
            var validatore = validatori.FirstOrDefault(v => v.Nome == nome);
            if (validatore is null)
                continue;

            try
            {
                messaggi.AddRange(validatore.Valida(descrittore, dati).Problemi.Select(p => $"{p.Percorso}: {p.Messaggio}"));
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                messaggi.Add($"Il controllo «{NomiModello.Nome(nome)}» non si può eseguire su questo contenuto.");
            }
        }

        return messaggi;
    }
}
