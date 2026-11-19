namespace DBEnigmistica.Domain.Diritti;

/// <summary>Prova che un Gioco è di pubblico dominio: autore, anno e motivo.</summary>
public sealed record ProvaPubblicoDominio
{
    private ProvaPubblicoDominio()
    {
    }

    public ProvaPubblicoDominio(string autore, int anno, string motivo)
    {
        if (anno < -3000 || anno > DateTime.UtcNow.Year)
            throw new ArgumentException("L'anno deve essere compreso tra -3000 e l'anno corrente.", nameof(anno));
        Autore = Controlli.Testo(autore, nameof(autore), 200);
        Anno = anno;
        Motivo = Controlli.Testo(motivo, nameof(motivo), 1000);
    }

    public string Autore { get; private init; } = string.Empty;

    public int Anno { get; private init; }

    public string Motivo { get; private init; } = string.Empty;
}
