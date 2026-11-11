using System.Runtime.CompilerServices;
using DBEnigmistica.Application.Registro;
using DBEnigmistica.Console.Cli;
using DBEnigmistica.Console.Commands;
using DBEnigmistica.Domain.Registro;
using FluentAssertions;

namespace DBEnigmistica.Tests.Unit.Cli;

public class VerifyIntegrityCommandTests
{
    private sealed class RegistroFinto(IReadOnlyList<EventoRegistro> righe) : IRegistroEventi
    {
        public Task<EventoRegistro> AggiungiAsync(NuovoEvento evento, CancellationToken ct = default) =>
            throw new NotSupportedException();

        public async IAsyncEnumerable<EventoRegistro> LeggiInOrdineAsync([EnumeratorCancellation] CancellationToken ct = default)
        {
            foreach (var riga in righe)
            {
                await Task.Yield();
                yield return riga;
            }
        }
    }

    private static List<EventoRegistro> Catena(int n)
    {
        var righe = new List<EventoRegistro>();
        var precedente = ImprontaEvento.Iniziale;
        for (var i = 1; i <= n; i++)
        {
            var evento = new NuovoEvento(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), "tipo", "entita", i.ToString(), "autore", "{}");
            var impronta = ImprontaEvento.Calcola(precedente, evento);
            righe.Add(EventoRegistro.Da(i, evento, precedente, impronta));
            precedente = impronta;
        }

        return righe;
    }

    private static async Task<(int Code, string Out, string Error)> RunAsync(List<EventoRegistro> righe)
    {
        var output = new StringWriter();
        var error = new StringWriter();
        var handler = new VerifyIntegrityHandler(
            new VerificaIntegritaRegistro(new RegistroFinto(righe)), new CliStreams(output, error));

        var code = await handler.ExecuteAsync(CancellationToken.None);
        return (code, output.ToString(), error.ToString());
    }

    [Fact]
    public async Task IntactRegistry_ExitsWithZeroAndReportsCount()
    {
        var result = await RunAsync(Catena(3));

        result.Code.Should().Be(0);
        result.Out.Should().Contain("Registro integro").And.Contain("3 eventi verificati");
        result.Error.Should().BeEmpty();
    }

    [Fact]
    public async Task EmptyRegistry_IsIntact()
    {
        var result = await RunAsync([]);

        result.Code.Should().Be(0);
        result.Out.Should().Contain("0 eventi verificati");
        result.Error.Should().BeEmpty();
    }

    [Fact]
    public async Task AlteredRegistry_ExitsWithOneAndNamesTheRowAndTheReason()
    {
        var righe = Catena(3);
        righe[1] = righe[1] with { Dati = "{\"a\":1}" };

        var result = await RunAsync(righe);

        result.Code.Should().Be(ExitCodes.Error);
        result.Error.Should().Contain("Registro alterato").And.Contain("id 2").And.Contain(VerificaCatena.MotivoRicalcolata);
        result.Out.Should().BeEmpty();
    }
}
