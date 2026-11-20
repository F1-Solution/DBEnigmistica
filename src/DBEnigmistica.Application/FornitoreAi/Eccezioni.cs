namespace DBEnigmistica.Application.FornitoreAi;

/// <summary>Il numero massimo di chiamate dell'esecuzione è stato raggiunto (codice d'uscita 4).</summary>
public sealed class LimiteEsecuzioneRaggiuntoException()
    : InvalidOperationException("Limite dell'esecuzione raggiunto");

/// <summary>In modalità Riproduci manca la Registrazione della pagina.</summary>
public sealed class RegistrazioneMancanteException(ChiaveRegistrazione chiave)
    : InvalidOperationException($"Registrazione mancante: {chiave}");

/// <summary>La Registrazione esiste ma non si può usare: file illeggibile, SHA-256 diverso o risoluzione diversa.</summary>
public sealed class RegistrazioneNonValidaException(string dettaglio, Exception? inner = null)
    : InvalidOperationException($"Registrazione non valida: {dettaglio}", inner);
