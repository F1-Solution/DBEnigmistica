using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace DBEnigmistica.Infrastructure.Prototipo;

/// <summary>
/// Ogni connessione del prototipo lavora con il ruolo <c>dbe_creazione</c> (<c>SET ROLE</c>): stessa stringa di connessione,
/// ma lo schema <c>riviste</c> resta illeggibile (ADR 001, criterio 3).
/// </summary>
public sealed class RuoloCreazioneInterceptor : DbConnectionInterceptor
{
    public const string Ruolo = "dbe_creazione";

    public override void ConnectionOpened(DbConnection connection, ConnectionEndEventData eventData)
    {
        ImpostaRuolo(connection);
        base.ConnectionOpened(connection, eventData);
    }

    public override async Task ConnectionOpenedAsync(DbConnection connection, ConnectionEndEventData eventData, CancellationToken cancellationToken = default)
    {
        await using var comando = connection.CreateCommand();
        comando.CommandText = $"SET ROLE {Ruolo}";
        await comando.ExecuteNonQueryAsync(cancellationToken);
        await base.ConnectionOpenedAsync(connection, eventData, cancellationToken);
    }

    private static void ImpostaRuolo(DbConnection connection)
    {
        using var comando = connection.CreateCommand();
        comando.CommandText = $"SET ROLE {Ruolo}";
        comando.ExecuteNonQuery();
    }
}
