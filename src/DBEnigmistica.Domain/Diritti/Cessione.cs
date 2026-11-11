namespace DBEnigmistica.Domain.Diritti;

/// <summary>Cessione del Gioco a un cliente, per un periodo e un territorio.</summary>
public sealed class Cessione
{
    private Cessione()
    {
    }

    public Guid Id { get; private init; }

    public Guid GiocoId { get; private init; }

    public Guid ClienteId { get; private init; }

    public DateTime StipulataIl { get; private init; }

    public Periodo Periodo { get; private init; } = null!;

    public bool Esclusiva { get; private init; }

    public Territorio Territorio { get; private init; } = null!;

    /// <summary>Il Gioco è stato promesso o venduto come inedito.</summary>
    public bool ComeInedito { get; private init; }

    public static Cessione Crea(
        Guid giocoId, Guid clienteId, DateTime stipulataIl, Periodo periodo,
        bool esclusiva, Territorio territorio, bool comeInedito) =>
        new()
        {
            Id = Guid.CreateVersion7(),
            GiocoId = Controlli.Id(giocoId, nameof(giocoId)),
            ClienteId = Controlli.Id(clienteId, nameof(clienteId)),
            StipulataIl = Controlli.Utc(stipulataIl, nameof(stipulataIl)),
            Periodo = periodo ?? throw new ArgumentNullException(nameof(periodo)),
            Esclusiva = esclusiva,
            Territorio = territorio ?? throw new ArgumentNullException(nameof(territorio)),
            ComeInedito = comeInedito
        };

    /// <summary>Vero se le due Cessioni sono esclusive e si toccano per periodo e territorio.</summary>
    public bool InConflittoEsclusivoCon(Cessione altra) =>
        Esclusiva && altra.Esclusiva && Periodo.SiSovrappone(altra.Periodo) && Territorio.SiSovrappone(altra.Territorio);
}
