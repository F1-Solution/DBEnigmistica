using System.Runtime.CompilerServices;
using DBEnigmistica.Application.Registro;
using DBEnigmistica.Domain.Registro;
using FluentAssertions;

namespace DBEnigmistica.Tests.Unit.Application;

public class VerificaIntegritaRegistroTests
{
    private sealed class RegistroFinto(IReadOnlyList<EventoRegistro> righe) : IRegistroEventi
    {
        public Task<EventoRegistro> AggiungiAsync(NuovoEvento evento, CancellationToken ct = default) =>
            throw new NotSupportedException();

        public async IAsyncEnumerable<EventoRegistro> LeggiInOrdineAsync(
            [EnumeratorCancellation] CancellationToken ct = default)
        {
            foreach (var r in righe)
            {
                await Task.Yield();
                yield return r;
            }
        }
    }

    private static List<EventoRegistro> Catena(int n)
    {
        var t = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var righe = new List<EventoRegistro>();
        var prec = ImprontaEvento.Iniziale;
        for (var i = 1; i <= n; i++)
        {
            var e = new NuovoEvento(t, "tipo", "entita", i.ToString(), "autore", "{}");
            var h = ImprontaEvento.Calcola(prec, e);
            righe.Add(EventoRegistro.Da(i, e, prec, h));
            prec = h;
        }
        return righe;
    }

    [Fact]
    public async Task Reports_an_intact_registry()
    {
        var esito = await new VerificaIntegritaRegistro(new RegistroFinto(Catena(2))).EseguiAsync();
        esito.Integra.Should().BeTrue();
        esito.EventiVerificati.Should().Be(2);
    }

    [Fact]
    public async Task Reports_the_altered_row()
    {
        var righe = Catena(3);
        righe[0] = righe[0] with { Dati = "{\"a\":1}" };
        var esito = await new VerificaIntegritaRegistro(new RegistroFinto(righe)).EseguiAsync();
        esito.Integra.Should().BeFalse();
        esito.PrimoIdNonValido.Should().Be(1);
    }
}
