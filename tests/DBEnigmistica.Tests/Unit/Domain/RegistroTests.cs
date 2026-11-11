using System.Security.Cryptography;
using DBEnigmistica.Domain.Registro;
using FluentAssertions;

namespace DBEnigmistica.Tests.Unit.Domain;

public class RegistroTests
{
    private static readonly DateTime T0 = new(2026, 11, 11, 10, 20, 30, DateTimeKind.Utc);

    private static NuovoEvento Evento(string entitaId = "g-1", string dati = "{}", string autore = "Alessio") =>
        new(T0.AddTicks(1234560), "gioco_creato", "gioco_originale", entitaId, autore, dati);

    private static List<EventoRegistro> Catena(int n)
    {
        var righe = new List<EventoRegistro>();
        var prec = ImprontaEvento.Iniziale;
        for (var i = 1; i <= n; i++)
        {
            var e = Evento($"g-{i}");
            var h = ImprontaEvento.Calcola(prec, e);
            righe.Add(EventoRegistro.Da(i, e, prec, h));
            prec = h;
        }
        return righe;
    }

    [Fact]
    public void Initial_hash_is_32_zero_bytes()
    {
        ImprontaEvento.Iniziale.Should().Equal(new byte[32]);
    }

    [Fact]
    public void Hash_matches_the_fixed_vector()
    {
        var e = new NuovoEvento(T0.AddTicks(1234560),
            "gioco_creato", "gioco_originale", "g-1", "Alessio", "{\"titolo\":\"è\"}");

        var hex = Convert.ToHexString(ImprontaEvento.Calcola(ImprontaEvento.Iniziale, e)).ToLowerInvariant();

        hex.Should().Be("ba996fd89b703561f5a0c4b99e5ce1ce231ee663eb977f18c9551daf80c1eada");
    }

    [Fact]
    public void Content_uses_length_prefixed_fields_so_boundaries_are_unambiguous()
    {
        var a = ImprontaEvento.Contenuto(Evento("ab", "{}", "c"));
        var b = ImprontaEvento.Contenuto(Evento("a", "{}", "bc"));
        a.Should().NotEqual(b);
    }

    [Fact]
    public void Hash_depends_on_the_previous_hash()
    {
        var e = Evento();
        ImprontaEvento.Calcola(ImprontaEvento.Iniziale, e)
            .Should().NotEqual(ImprontaEvento.Calcola(SHA256.HashData([1]), e));
    }

    [Fact]
    public void Hash_rejects_a_previous_hash_that_is_not_32_bytes()
    {
        var act = () => ImprontaEvento.Calcola(new byte[5], Evento());
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void NuovoEvento_truncates_to_the_microsecond_and_requires_utc()
    {
        Evento().AvvenutoIl.Ticks.Should().Be(T0.AddTicks(1234560).Ticks);
        var troncato = new NuovoEvento(T0.AddTicks(7), "t", "e", "1", "a", "{}");
        troncato.AvvenutoIl.Should().Be(T0);
        var act = () => new NuovoEvento(new DateTime(2026, 1, 1), "t", "e", "1", "a", "{}");
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("Tipo", "e", "1", "a", "{}")]
    [InlineData("t", "E x", "1", "a", "{}")]
    [InlineData("t", "e", "", "a", "{}")]
    [InlineData("t", "e", "1", " ", "{}")]
    [InlineData("t", "e", "1", "a", "[]")]
    [InlineData("t", "e", "1", "a", "non json")]
    [InlineData("t", "e", "1", "a", "")]
    public void NuovoEvento_validates_its_fields(string tipo, string entita, string id, string autore, string dati)
    {
        var act = () => new NuovoEvento(T0, tipo, entita, id, autore, dati);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Intact_chain_is_verified()
    {
        var esito = VerificaCatena.Verifica(Catena(3));
        esito.Integra.Should().BeTrue();
        esito.EventiVerificati.Should().Be(3);
        esito.PrimoIdNonValido.Should().BeNull();
        esito.Motivo.Should().BeNull();
    }

    [Fact]
    public void Empty_chain_is_intact()
    {
        var esito = VerificaCatena.Verifica([]);
        esito.Integra.Should().BeTrue();
        esito.EventiVerificati.Should().Be(0);
    }

    [Fact]
    public void Altered_data_is_reported_with_the_row_id()
    {
        var righe = Catena(3);
        righe[1] = righe[1] with { Dati = "{\"x\":1}" };

        var esito = VerificaCatena.Verifica(righe);

        esito.Integra.Should().BeFalse();
        esito.PrimoIdNonValido.Should().Be(2);
        esito.Motivo.Should().Be("impronta ricalcolata diversa");
        esito.EventiVerificati.Should().Be(1);
    }

    [Fact]
    public void Missing_link_is_reported_on_the_next_row()
    {
        var righe = Catena(3);
        righe.RemoveAt(1);

        var esito = VerificaCatena.Verifica(righe);

        esito.Integra.Should().BeFalse();
        esito.PrimoIdNonValido.Should().Be(3);
        esito.Motivo.Should().Be("impronta precedente non coincide");
    }

    [Fact]
    public void Missing_first_row_is_reported()
    {
        var righe = Catena(2);
        righe.RemoveAt(0);
        VerificaCatena.Verifica(righe).PrimoIdNonValido.Should().Be(2);
    }

    [Theory]
    [InlineData("", "autore")]
    [InlineData("1", "	")]
    public async Task Row_that_fails_domain_validation_is_reported_without_throwing(string entitaId, string autore)
    {
        var righe = Catena(3);
        righe[1] = righe[1] with { EntitaId = entitaId, Autore = autore };

        var sincrono = VerificaCatena.Verifica(righe);
        var asincrono = await VerificaCatena.VerificaAsync(ComeAsync(righe));

        foreach (var esito in new[] { sincrono, asincrono })
        {
            esito.Integra.Should().BeFalse();
            esito.PrimoIdNonValido.Should().Be(2);
            esito.Motivo.Should().Be(VerificaCatena.MotivoRigaNonValida);
            esito.EventiVerificati.Should().Be(1);
        }
    }

    [Fact]
    public async Task Async_verification_gives_the_same_outcome()
    {
        var righe = Catena(3);
        righe[2] = righe[2] with { Autore = "Intruso" };

        var esito = await VerificaCatena.VerificaAsync(ComeAsync(righe));

        esito.PrimoIdNonValido.Should().Be(3);
    }

    private static async IAsyncEnumerable<EventoRegistro> ComeAsync(IEnumerable<EventoRegistro> righe)
    {
        foreach (var r in righe)
        {
            await Task.Yield();
            yield return r;
        }
    }
}
