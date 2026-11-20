namespace DBEnigmistica.Application.FornitoreAi;

/// <summary>Porta di registrazione delle chiamate al Fornitore AI; la persistenza arriva nella 1.3.</summary>
public interface IRegistroChiamateAi
{
    Task RegistraAsync(ChiamataAi chiamata, CancellationToken cancellationToken = default);
}

/// <summary>Registro delle chiamate tenuto in memoria.</summary>
public sealed class RegistroChiamateAiInMemoria : IRegistroChiamateAi
{
    private readonly List<ChiamataAi> _chiamate = [];
    private readonly object _blocco = new();

    public IReadOnlyList<ChiamataAi> Chiamate
    {
        get
        {
            lock (_blocco)
                return [.. _chiamate];
        }
    }

    public Task RegistraAsync(ChiamataAi chiamata, CancellationToken cancellationToken = default)
    {
        lock (_blocco)
            _chiamate.Add(chiamata);
        return Task.CompletedTask;
    }
}
