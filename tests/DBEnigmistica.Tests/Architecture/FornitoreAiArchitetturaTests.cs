using System.Reflection;
using DBEnigmistica.AI.Configuration;
using DBEnigmistica.Application.FornitoreAi;
using DBEnigmistica.Domain.Entities;
using FluentAssertions;

namespace DBEnigmistica.Tests.Architecture;

/// <summary>Le porte del Fornitore AI stanno in Application e i pacchetti dei fornitori solo in DBEnigmistica.AI (ADR 004).</summary>
public class FornitoreAiArchitetturaTests
{
    private static readonly Assembly Domain = typeof(Game).Assembly;
    private static readonly Assembly Application = typeof(IFornitoreAiEstrazione).Assembly;
    private static readonly Assembly Ai = typeof(FornitoreAiOptions).Assembly;

    private static IEnumerable<string> TipiDelleInterfacce(Assembly assembly) =>
        assembly.GetTypes()
            .Where(t => t.Namespace == "DBEnigmistica.Domain.Interfaces")
            .Select(t => t.Name);

    [Fact]
    public void NessunaPortaDelFornitoreAi_ESuDomainInterfaces()
    {
        string[] vietate = ["IFornitoreAiEstrazione", "IRegistroChiamateAi", "IAiGameAnalyzer", "IValidatoreSchemaJson"];

        TipiDelleInterfacce(Domain).Should().NotIntersectWith(vietate);
    }

    [Fact]
    public void NessunaPortaDelPdf_ESuDomainInterfaces()
    {
        string[] vietate = ["IPdfService", "IRenderPagine", "IArchivio"];

        TipiDelleInterfacce(Domain).Should().NotIntersectWith(vietate);
    }

    [Fact]
    public void LePorteDelFornitoreAi_StannoInApplication()
    {
        Application.GetTypes().Where(t => t.IsInterface).Select(t => t.Name)
            .Should().Contain(["IFornitoreAiEstrazione", "IRegistroChiamateAi", "IAiGameAnalyzer"]);
    }

    [Fact]
    public void SoloIlProgettoAi_ReferenziaPacchettiDeiFornitori()
    {
        foreach (var assembly in new[] { Domain, Application, typeof(DBEnigmistica.Infrastructure.Configuration.DependencyInjection).Assembly, typeof(DBEnigmistica.Console.Cli.CliApp).Assembly })
        {
            assembly.GetReferencedAssemblies().Select(a => a.Name ?? string.Empty)
                .Should().NotContain(n => n.StartsWith("Anthropic", StringComparison.OrdinalIgnoreCase),
                    "i pacchetti dei fornitori stanno solo in DBEnigmistica.AI ({0})", assembly.GetName().Name);
        }
    }

    [Fact]
    public void Ai_DipendeDaApplicationEApplicationNonDipendeDaAi()
    {
        Ai.GetReferencedAssemblies().Select(a => a.Name).Should().Contain("DBEnigmistica.Application");
        Application.GetReferencedAssemblies().Select(a => a.Name).Should().NotContain("DBEnigmistica.AI");
    }
}
