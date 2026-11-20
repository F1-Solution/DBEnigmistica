using System.Security.Cryptography;
using System.Text;
using DBEnigmistica.Application.FornitoreAi;

namespace DBEnigmistica.Tests.Unit.FornitoreAi;

/// <summary>Fornitore AI finto e configurabile per i test: risponde con un JSON fisso e conta le chiamate.</summary>
public sealed class FornitoreAiFinto : IFornitoreAiEstrazione
{
    private int _chiamate;

    public string IdFornitore => "finto";

    public string Modello => "modello-finto";

    public EsitoEstrazione Esito { get; init; } = EsitoEstrazione.Riuscita;

    public string Json { get; init; } = "{\"giochi\":[]}";

    /// <summary>Testo grezzo che il fornitore finto "ha ricevuto": l'uscita JSON di Claude Code.</summary>
    public string Grezza { get; init; } = "{\"type\":\"result\",\"is_error\":false,\"result\":\"\",\"structured_output\":{\"giochi\":[]}}";

    /// <summary>Chiamate reali per pagina (retry e correzioni comprese).</summary>
    public int ChiamatePerPagina { get; init; } = 1;

    public int Chiamate => _chiamate;

    public Task<RispostaEstrazionePagina> EstraiPaginaAsync(RichiestaEstrazionePagina richiesta, CancellationToken cancellationToken = default)
    {
        Interlocked.Increment(ref _chiamate);
        var chiamate = Enumerable.Range(1, ChiamatePerPagina).Select(numero => new ChiamataAi(
            Guid.CreateVersion7(), DateTimeOffset.UnixEpoch, 5, ChiamataAi.UsoEstrazione, IdFornitore, Modello,
            richiesta.VersionePrompt, SHA256.HashData(Encoding.UTF8.GetBytes(richiesta.ImprontaPagina)), numero, false, Esito,
            null, null, null, 1, 1, null, null, 0.5m, "USD", "indicativo", false, Grezza, SHA256.HashData(Encoding.UTF8.GetBytes(Grezza)))).ToList();
        return Task.FromResult(new RispostaEstrazionePagina(Esito, Esito == EsitoEstrazione.Riuscita ? Json : null, null, null, chiamate));
    }
}
