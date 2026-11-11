using System.Reflection;
using DBEnigmistica.Domain.Entities;
using FluentAssertions;
using NetArchTest.Rules;

namespace DBEnigmistica.Tests.Architecture;

/// <summary>Regole di dipendenza tra i livelli (ROADMAP 2.1): le dipendenze vanno verso l'interno.</summary>
public class DipendenzeTests
{
    private static readonly Assembly Domain = typeof(Game).Assembly;
    private static readonly Assembly Application = typeof(DBEnigmistica.Application.Services.Export.DataExportService).Assembly;

    private static string Elenco(TestResult risultato) =>
        string.Join(", ", risultato.FailingTypeNames ?? []);

    [Fact]
    public void Controllo_RiconosceUnaDipendenzaVera()
    {
        var tipi = Types.InAssembly(Application).That().HaveDependencyOn("DBEnigmistica.Domain").GetTypes();

        tipi.Should().NotBeEmpty("senza questo il motore di analisi potrebbe passare a vuoto");
    }

    [Fact]
    public void Domain_NonDipendeDaAltriLivelliNeDaPacchettiDiInfrastruttura()
    {
        string[] vietati =
        [
            "DBEnigmistica.Application",
            "DBEnigmistica.Infrastructure",
            "DBEnigmistica.AI",
            "DBEnigmistica.Console",
            "Microsoft.EntityFrameworkCore",
            "Npgsql",
        ];

        var risultato = Types.InAssembly(Domain).ShouldNot().HaveDependencyOnAny(vietati).GetResult();

        risultato.IsSuccessful.Should().BeTrue("il Domain non deve dipendere da: {0}", Elenco(risultato));
    }

    [Fact]
    public void Application_NonDipendeDaInfrastrutturaInterfacciaNeFornitoreAi()
    {
        string[] vietati =
        [
            "Microsoft.EntityFrameworkCore",
            "Npgsql",
            "Spectre.Console",
            "System.CommandLine",
            "Anthropic",
            "DBEnigmistica.Infrastructure",
            "DBEnigmistica.AI",
            "DBEnigmistica.Console",
        ];

        var risultato = Types.InAssembly(Application).ShouldNot().HaveDependencyOnAny(vietati).GetResult();

        risultato.IsSuccessful.Should().BeTrue("l'Application non deve dipendere da: {0}", Elenco(risultato));
    }

    [Fact]
    public void Catalogo_NonDipendeDalleRiviste()
    {
        var risultato = Types.InAssembly(Domain)
            .That().ResideInNamespaceStartingWith("DBEnigmistica.Domain.Catalogo")
            .ShouldNot().HaveDependencyOn("DBEnigmistica.Domain.Entities")
            .GetResult();

        risultato.IsSuccessful.Should().BeTrue("la creazione non vede le Riviste: {0}", Elenco(risultato));
    }
}
