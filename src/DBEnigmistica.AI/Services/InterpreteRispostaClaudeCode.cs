using DBEnigmistica.Application.FornitoreAi;

namespace DBEnigmistica.AI.Services;

/// <summary>Legge il testo grezzo di una Registrazione con lo stesso analizzatore dell'adattatore dal vivo.</summary>
public sealed class InterpreteRispostaClaudeCode : IInterpreteRispostaGrezza
{
    public InterpretazioneRisposta Interpreta(string rispostaGrezza)
    {
        // Si registrano solo risposte arrivate con codice d'uscita 0 e senza errori su stderr.
        var uscita = UscitaClaudeCode.Analizza(rispostaGrezza, string.Empty, 0, TimeProvider.System);
        return uscita.Tipo == TipoUscitaClaudeCode.Successo
            ? new InterpretazioneRisposta(uscita.Json, uscita.Messaggio)
            : new InterpretazioneRisposta(null, uscita.Messaggio);
    }
}
