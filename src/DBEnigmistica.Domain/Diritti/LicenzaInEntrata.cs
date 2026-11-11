namespace DBEnigmistica.Domain.Diritti;

/// <summary>Licenza con cui un licenziante concede il Gioco per un periodo.</summary>
public sealed class LicenzaInEntrata
{
    private LicenzaInEntrata()
    {
    }

    public Guid Id { get; private init; }

    public Guid GiocoId { get; private init; }

    public Guid LicenzianteId { get; private init; }

    public string EstremiAccordo { get; private init; } = string.Empty;

    public Periodo Periodo { get; private init; } = null!;

    public static LicenzaInEntrata Crea(Guid giocoId, Guid licenzianteId, string estremiAccordo, Periodo periodo) =>
        new()
        {
            Id = Guid.CreateVersion7(),
            GiocoId = Controlli.Id(giocoId, nameof(giocoId)),
            LicenzianteId = Controlli.Id(licenzianteId, nameof(licenzianteId)),
            EstremiAccordo = Controlli.Testo(estremiAccordo, nameof(estremiAccordo), 500),
            Periodo = periodo ?? throw new ArgumentNullException(nameof(periodo))
        };
}
