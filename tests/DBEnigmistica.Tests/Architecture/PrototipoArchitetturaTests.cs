using DBEnigmistica.Application.Creazione;
using DBEnigmistica.Web.Api;
using FluentAssertions;
using NetArchTest.Rules;

namespace DBEnigmistica.Tests.Architecture;

/// <summary>Confini del prototipo (1b.1): Web, Creazione e Algoritmi non si appoggiano a ciò che non devono.</summary>
public class PrototipoArchitetturaTests
{
    private const string NamespaceCreazione = "DBEnigmistica.Application.Creazione";
    private const string NamespaceAlgoritmi = "DBEnigmistica.Application.Creazione.Algoritmi";

    private static string Elenco(TestResult risultato) => string.Join(", ", risultato.FailingTypeNames ?? []);

    [Fact]
    public void Web_DipendeDaApplicationEDagliAdattatoriSoloNellaComposizione()
    {
        string[] vietati =
        [
            "DBEnigmistica.Infrastructure", "DBEnigmistica.AI", "DBEnigmistica.Console", "Microsoft.EntityFrameworkCore", "Npgsql",
            "DBEnigmistica.Domain.Entities",
        ];

        var risultato = Types.InAssembly(typeof(PrototipoEndpoints).Assembly)
            .That().ResideInNamespaceStartingWith("DBEnigmistica.Web")
            .And().DoNotResideInNamespaceStartingWith("DBEnigmistica.Web.Composizione")
            .ShouldNot().HaveDependencyOnAny(vietati)
            .GetResult();

        risultato.IsSuccessful.Should().BeTrue("fuori da Composizione Web usa solo Application: {0}", Elenco(risultato));
    }

    [Fact]
    public void Controllo_RiconosceLeDipendenzeDellaComposizione()
    {
        Types.InAssembly(typeof(PrototipoEndpoints).Assembly)
            .That().ResideInNamespaceStartingWith("DBEnigmistica.Web.Composizione")
            .And().HaveDependencyOnAny("DBEnigmistica.Infrastructure", "DBEnigmistica.AI")
            .GetTypes().Should().NotBeEmpty("senza questo il test potrebbe passare a vuoto");
    }

    [Fact]
    public void Creazione_NonDipendeDaAcquisizioneNeDaTipiDelloSchemaRiviste()
    {
        var risultato = Types.InAssembly(typeof(GeneraCandidato).Assembly)
            .That().ResideInNamespaceStartingWith(NamespaceCreazione)
            .And().DoNotResideInNamespaceStartingWith(NamespaceAlgoritmi)
            .ShouldNot().HaveDependencyOnAny("DBEnigmistica.Application.Acquisizione", "DBEnigmistica.Domain.Entities")
            .GetResult();

        risultato.IsSuccessful.Should().BeTrue("la creazione non legge le Riviste: {0}", Elenco(risultato));
    }

    [Fact]
    public void Algoritmi_NonDipendeDalFornitoreAi()
    {
        var algoritmi = Types.InAssembly(typeof(GeneraCandidato).Assembly).That().ResideInNamespaceStartingWith(NamespaceAlgoritmi);

        algoritmi.GetTypes().Should().NotBeEmpty();
        var risultato = algoritmi.ShouldNot().HaveDependencyOnAny("DBEnigmistica.Application.FornitoreAi", "DBEnigmistica.AI").GetResult();

        risultato.IsSuccessful.Should().BeTrue("i generatori sono C# puro: {0}", Elenco(risultato));
    }

    [Fact]
    public void InfrastrutturaDelPrototipo_NonUsaTipiDelloSchemaRiviste()
    {
        var risultato = Types.InAssembly(typeof(DBEnigmistica.Infrastructure.Prototipo.CandidatiRepository).Assembly)
            .That().ResideInNamespaceStartingWith("DBEnigmistica.Infrastructure.Prototipo")
            .ShouldNot().HaveDependencyOnAny("DBEnigmistica.Domain.Entities")
            .GetResult();

        risultato.IsSuccessful.Should().BeTrue("{0}", Elenco(risultato));
    }
}
