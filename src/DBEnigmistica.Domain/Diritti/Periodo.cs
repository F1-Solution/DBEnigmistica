namespace DBEnigmistica.Domain.Diritti;

/// <summary>Intervallo di tempo semiaperto [Inizio, Fine) in UTC; senza Fine non scade mai.</summary>
public sealed record Periodo
{
    private Periodo()
    {
    }

    public Periodo(DateTime inizio, DateTime? fine)
    {
        Inizio = Controlli.Utc(inizio, nameof(inizio));
        Fine = fine is null ? null : Controlli.Utc(fine.Value, nameof(fine));
        if (Fine <= Inizio)
            throw new ArgumentException("La fine del periodo deve essere successiva all'inizio.", nameof(fine));
    }

    public DateTime Inizio { get; private init; }

    public DateTime? Fine { get; private init; }

    public bool Contiene(DateTime istante) => istante >= Inizio && (Fine is null || istante < Fine);

    public bool SiSovrappone(Periodo altro) =>
        Inizio < (altro.Fine ?? DateTime.MaxValue) && altro.Inizio < (Fine ?? DateTime.MaxValue);

    public bool TerminatoPrima(DateTime istante) => Fine is not null && Fine <= istante;
}
