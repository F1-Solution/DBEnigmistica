namespace DBEnigmistica.Domain.Catalogo;

/// <summary>Quando e dove un Gioco è stato pubblicato per la prima volta.</summary>
public sealed record PrimaPubblicazione
{
    public PrimaPubblicazione(DateTime il, string luogo)
    {
        Il = Controlli.Utc(il, nameof(il));
        Luogo = Controlli.Testo(luogo, nameof(luogo), 300);
    }

    public DateTime Il { get; }

    public string Luogo { get; }
}

/// <summary>Due stati senza ritorno: inedito finché non esiste una prima pubblicazione.</summary>
public sealed record StatoInedito
{
    private StatoInedito(PrimaPubblicazione? prima) => Prima = prima;

    public static StatoInedito Inedito { get; } = new((PrimaPubblicazione?)null);

    public PrimaPubblicazione? Prima { get; }

    public bool EInedito => Prima is null;

    public static StatoInedito Pubblicato(PrimaPubblicazione prima) =>
        new(prima ?? throw new ArgumentNullException(nameof(prima)));

    public StatoInedito Pubblica(PrimaPubblicazione prima)
    {
        ArgumentNullException.ThrowIfNull(prima);
        if (Prima is not null)
            throw new InvalidOperationException(
                $"Il Gioco è già stato pubblicato il {Prima.Il:yyyy-MM-dd} in {Prima.Luogo}: lo stato di inedito non torna indietro.");
        return new StatoInedito(prima);
    }
}
