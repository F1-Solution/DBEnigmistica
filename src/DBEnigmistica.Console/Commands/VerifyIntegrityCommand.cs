using System.CommandLine;
using DBEnigmistica.Application.Registro;
using DBEnigmistica.Console.Cli;

namespace DBEnigmistica.Console.Commands;

public class VerifyIntegrityCommand : Command
{
    public VerifyIntegrityCommand(CommandRunner runner)
        : base("verify-integrity", "Verifica la catena delle impronte del registro in sola aggiunta")
    {
        this.SetAction((_, ct) => runner.RunAsync<VerifyIntegrityHandler>(
            HostRequirements.Database,
            (handler, token) => handler.ExecuteAsync(token),
            ct));
    }
}

/// <summary>Esito 0 se la catena delle impronte è integra, 1 con l'id del primo evento non valido se è alterata.</summary>
public sealed class VerifyIntegrityHandler(VerificaIntegritaRegistro verifica, CliStreams streams)
{
    public async Task<int> ExecuteAsync(CancellationToken cancellationToken)
    {
        var esito = await verifica.EseguiAsync(cancellationToken);

        if (esito.Integra)
        {
            streams.Out.WriteLine($"Registro integro: {esito.EventiVerificati} eventi verificati.");
            return ExitCodes.Success;
        }

        streams.Error.WriteLine(
            $"Registro alterato: primo evento non valido con id {esito.PrimoIdNonValido} ({esito.Motivo}). " +
            $"Eventi verificati prima: {esito.EventiVerificati}.");
        return ExitCodes.Error;
    }
}
