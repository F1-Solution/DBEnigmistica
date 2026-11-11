namespace DBEnigmistica.Domain;

/// <summary>Controlli condivisi dalle invarianti del dominio; ogni errore è un ArgumentException in italiano.</summary>
internal static class Controlli
{
    /// <summary>Verifica che la data sia UTC e la tronca al microsecondo, la precisione di PostgreSQL.</summary>
    public static DateTime Utc(DateTime valore, string nome)
    {
        if (valore.Kind != DateTimeKind.Utc)
            throw new ArgumentException($"La data «{nome}» deve essere in UTC.", nome);
        return new DateTime(valore.Ticks - valore.Ticks % 10, DateTimeKind.Utc);
    }

    public static string Testo(string? valore, string nome, int massimo)
    {
        if (string.IsNullOrWhiteSpace(valore))
            throw new ArgumentException($"Il campo «{nome}» è obbligatorio.", nome);
        return Massimo(valore, nome, massimo)!;
    }

    public static string? TestoFacoltativo(string? valore, string nome, int massimo) =>
        string.IsNullOrWhiteSpace(valore) ? null : Massimo(valore, nome, massimo);

    public static Guid Id(Guid valore, string nome) =>
        valore == Guid.Empty ? throw new ArgumentException($"Il campo «{nome}» è obbligatorio.", nome) : valore;

    private static string? Massimo(string valore, string nome, int massimo) =>
        valore.Length > massimo
            ? throw new ArgumentException($"Il campo «{nome}» supera {massimo} caratteri.", nome)
            : valore;
}
