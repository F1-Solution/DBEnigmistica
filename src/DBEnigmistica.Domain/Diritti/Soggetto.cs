namespace DBEnigmistica.Domain.Diritti;

public enum TipoSoggetto
{
    Persona = 1,
    Organizzazione = 2
}

/// <summary>Anagrafica unica di titolari, licenzianti e clienti.</summary>
public sealed class Soggetto
{
    private Soggetto()
    {
    }

    public Guid Id { get; private init; }

    public TipoSoggetto Tipo { get; private init; }

    public string Denominazione { get; private init; } = string.Empty;

    public string? IdentificativoFiscale { get; private init; }

    public string? Note { get; private init; }

    public DateTime CreatoIl { get; private init; }

    public static Soggetto Crea(
        TipoSoggetto tipo, string denominazione, string? identificativoFiscale, string? note, DateTime creatoIl)
    {
        if (!Enum.IsDefined(tipo))
            throw new ArgumentException("Il tipo di Soggetto non è valido.", nameof(tipo));

        return new Soggetto
        {
            Id = Guid.CreateVersion7(),
            Tipo = tipo,
            Denominazione = Controlli.Testo(denominazione, nameof(denominazione), 200),
            IdentificativoFiscale = Controlli.TestoFacoltativo(identificativoFiscale, nameof(identificativoFiscale), 32),
            Note = string.IsNullOrWhiteSpace(note) ? null : note,
            CreatoIl = Controlli.Utc(creatoIl, nameof(creatoIl))
        };
    }
}
