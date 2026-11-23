using DBEnigmistica.Tipologie.Modello;
using FluentAssertions;
using NetArchTest.Rules;

namespace DBEnigmistica.Tests.Architecture;

/// <summary>Il progetto Tipologie (R5, R6) è la foglia della soluzione: nessun progetto interno, nessun EF né Npgsql.</summary>
public class TipologieArchitetturaTests
{
    private static readonly System.Reflection.Assembly Tipologie = typeof(Descrittore).Assembly;

    [Fact]
    public void Controllo_RiconosceUnaDipendenzaVera()
    {
        Types.InAssembly(Tipologie).That().HaveDependencyOn("System.Text.Json").GetTypes()
            .Should().NotBeEmpty("senza questo il test potrebbe passare a vuoto");
    }

    [Fact]
    public void Tipologie_NonDipendeDaAltriProgettiNeDaEfONpgsql()
    {
        string[] vietati =
        [
            "DBEnigmistica.Application", "DBEnigmistica.Infrastructure", "DBEnigmistica.AI", "DBEnigmistica.Console",
            "DBEnigmistica.Domain", "DBEnigmistica.Web", "Microsoft.EntityFrameworkCore", "Npgsql",
        ];

        var risultato = Types.InAssembly(Tipologie).ShouldNot().HaveDependencyOnAny(vietati).GetResult();

        risultato.IsSuccessful.Should().BeTrue("{0}", string.Join(", ", risultato.FailingTypeNames ?? []));
    }

    [Fact]
    public void Tipologie_NonReferenziaAssemblyDelleAltreParti()
    {
        var riferiti = Tipologie.GetReferencedAssemblies().Select(a => a.Name ?? string.Empty).ToList();

        riferiti.Where(n => n.StartsWith("DBEnigmistica", StringComparison.Ordinal) || n.StartsWith("Npgsql", StringComparison.Ordinal)
                || n.StartsWith("Microsoft.EntityFrameworkCore", StringComparison.Ordinal))
            .Should().BeEmpty();
    }

    [Fact]
    public void Domain_NonDipendeDaTipologie()
    {
        var risultato = Types.InAssembly(typeof(DBEnigmistica.Domain.Entities.Game).Assembly).ShouldNot().HaveDependencyOn("DBEnigmistica.Tipologie").GetResult();

        risultato.IsSuccessful.Should().BeTrue("{0}", string.Join(", ", risultato.FailingTypeNames ?? []));
    }

    [Fact]
    public void Validatori_DipendonoSoloDalModello()
    {
        var validatori = Types.InAssembly(Tipologie).That().ResideInNamespace("DBEnigmistica.Tipologie.Validatori");

        validatori.GetTypes().Should().NotBeEmpty();
        var risultato = validatori.ShouldNot().HaveDependencyOnAny("DBEnigmistica.Tipologie.Caricamento", "DBEnigmistica.Tipologie.Generatore", "DBEnigmistica.Tipologie.Derivazione", "DBEnigmistica.Tipologie.Catalogo").GetResult();

        risultato.IsSuccessful.Should().BeTrue("i validatori leggono i dati con il Modello: {0}", string.Join(", ", risultato.FailingTypeNames ?? []));
    }

    [Fact]
    public void Modello_NonDipendeDaAltreParti()
    {
        var modello = Types.InAssembly(Tipologie).That().ResideInNamespace("DBEnigmistica.Tipologie.Modello");

        modello.GetTypes().Should().NotBeEmpty();
        var risultato = modello.ShouldNot().HaveDependencyOnAny("DBEnigmistica.Tipologie.Caricamento", "DBEnigmistica.Tipologie.Generatore", "DBEnigmistica.Tipologie.Derivazione", "DBEnigmistica.Tipologie.Catalogo", "DBEnigmistica.Tipologie.Validatori").GetResult();

        risultato.IsSuccessful.Should().BeTrue("{0}", string.Join(", ", risultato.FailingTypeNames ?? []));
    }

    [Fact]
    public void InfrastrutturaDelleTipologie_UsaNpgsqlParametrizzatoENonEntityFramework()
    {
        var tipi = Types.InAssembly(typeof(DBEnigmistica.Infrastructure.Tipologie.MigratoreTipologie).Assembly)
            .That().ResideInNamespace("DBEnigmistica.Infrastructure.Tipologie");

        tipi.GetTypes().Should().NotBeEmpty();
        var risultato = tipi.ShouldNot().HaveDependencyOnAny("Microsoft.EntityFrameworkCore", "DBEnigmistica.Domain.Entities", "DBEnigmistica.Infrastructure.Data").GetResult();

        risultato.IsSuccessful.Should().BeTrue("{0}", string.Join(", ", risultato.FailingTypeNames ?? []));
    }
}
