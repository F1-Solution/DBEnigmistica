using DBEnigmistica.Infrastructure.Prototipo;
using FluentAssertions;
using Microsoft.Extensions.Configuration;

namespace DBEnigmistica.Tests.Unit.Creazione;

/// <summary>La cartella dei descrittori si risolve rispetto all'eseguibile e la pubblicazione la porta con sé.</summary>
public class CartellaTipologieTests
{
    private static IConfiguration Config(string? valore) =>
        new ConfigurationBuilder().AddInMemoryCollection(valore is null ? [] : [new KeyValuePair<string, string?>("Prototipo:CartellaTipologie", valore)]).Build();

    [Fact]
    public void SenzaValore_SiRisolveRispettoAllaCartellaDellEseguibile()
    {
        DependencyInjectionPrototipo.CartellaTipologie(Config(null)).Should().Be(Path.Combine(AppContext.BaseDirectory, "tipologie"));
    }

    [Fact]
    public void ValoreEsplicito_RelativoSiRisolveDallaCartellaCorrente_AssolutoResta()
    {
        DependencyInjectionPrototipo.CartellaTipologie(Config("miei")).Should().Be(Path.GetFullPath("miei"));
        var assoluto = Path.Combine(Path.GetTempPath(), "descrittori");
        DependencyInjectionPrototipo.CartellaTipologie(Config(assoluto)).Should().Be(assoluto);
    }

    [Fact]
    public void LaBuildPortaIDescrittoriVersionatiMaNonIMetaschemi()
    {
        var cartella = DependencyInjectionPrototipo.CartellaTipologie(Config(null));

        foreach (var id in new[] { "crittografia-breve", "rebus", "sudoku", "futoshiki", "nonogramma" })
            File.Exists(Path.Combine(cartella, id, "v0.json")).Should().BeTrue(id);
        Directory.Exists(Path.Combine(cartella, "_meta")).Should().BeFalse("i meta-schemi e il prompt sono risorse incorporate");
    }
}
