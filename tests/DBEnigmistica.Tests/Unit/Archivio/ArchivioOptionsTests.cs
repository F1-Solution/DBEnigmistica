using DBEnigmistica.Infrastructure.Configuration;
using DBEnigmistica.Infrastructure.Services.Archivio;
using DBEnigmistica.Infrastructure.Services.Pdf;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DBEnigmistica.Tests.Unit.Archivio;

public class ArchivioOptionsTests
{
    private static readonly ArchivioOptionsValidator Validatore = new();

    private static string CartellaAssoluta => Path.Combine(Path.GetTempPath(), "archivio");

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Cartella_Mancante_ChiedeDiImpostareArchivioCartella(string? cartella)
    {
        var esito = Validatore.Validate(null, new ArchivioOptions { Cartella = cartella });

        esito.Failed.Should().BeTrue();
        esito.Failures.Should().ContainSingle().Which.Should().Contain("Archivio__Cartella").And.Contain("Archivio:Cartella");
    }

    [Fact]
    public void Cartella_Relativa_ERifiutata()
    {
        var esito = Validatore.Validate(null, new ArchivioOptions { Cartella = "archivio" });

        esito.Failed.Should().BeTrue();
        esito.Failures.Should().ContainSingle().Which.Should().Contain("assoluto");
    }

    [Fact]
    public void Cartella_Assoluta_EValida()
    {
        Validatore.Validate(null, new ArchivioOptions { Cartella = CartellaAssoluta }).Succeeded.Should().BeTrue();
    }

    private static ServiceProvider Costruisci(Dictionary<string, string?> valori)
    {
        var configurazione = new ConfigurationBuilder().AddInMemoryCollection(valori).Build();
        var services = new ServiceCollection();
        services.AddInfrastructure(configurazione);
        return services.BuildServiceProvider();
    }

    [Fact]
    public void Infrastruttura_SenzaArchivioCartella_LanciaOptionsValidationExceptionAllUso()
    {
        using var provider = Costruisci(new() { ["ConnectionStrings:DefaultConnection"] = "Host=x" });

        var act = () => provider.GetRequiredService<IOptions<ArchivioOptions>>().Value;

        act.Should().Throw<OptionsValidationException>().Which.Failures.Should().Contain(f => f.Contains("Archivio:Cartella"));
    }

    [Fact]
    public void Infrastruttura_ConArchivioCartella_LeggeLaChiave()
    {
        using var provider = Costruisci(new()
        {
            ["ConnectionStrings:DefaultConnection"] = "Host=x",
            ["Archivio:Cartella"] = CartellaAssoluta
        });

        provider.GetRequiredService<IOptions<ArchivioOptions>>().Value.Cartella.Should().Be(CartellaAssoluta);
    }

    [Fact]
    public void Infrastruttura_SenzaEstrazioneDpi_Usa200()
    {
        using var provider = Costruisci(new() { ["ConnectionStrings:DefaultConnection"] = "Host=x" });

        provider.GetRequiredService<IOptions<EstrazioneOptions>>().Value.Dpi.Should().Be(200);
    }

    [Fact]
    public void Infrastruttura_ConEstrazioneDpiFuoriIntervallo_LanciaAllUso()
    {
        using var provider = Costruisci(new()
        {
            ["ConnectionStrings:DefaultConnection"] = "Host=x",
            ["Estrazione:Dpi"] = "72"
        });

        var act = () => provider.GetRequiredService<IOptions<EstrazioneOptions>>().Value;

        act.Should().Throw<OptionsValidationException>();
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("200.5")]
    public void Infrastruttura_ConEstrazioneDpiNonNumerico_LanciaAllUsoDicendoChiEIlValore(string valore)
    {
        using var provider = Costruisci(new()
        {
            ["ConnectionStrings:DefaultConnection"] = "Host=x",
            ["Estrazione:Dpi"] = valore
        });

        var act = () => provider.GetRequiredService<IOptions<EstrazioneOptions>>().Value;

        act.Should().Throw<OptionsValidationException>().Which.Failures.Should().ContainSingle()
            .Which.Should().Contain("Estrazione:Dpi").And.Contain("numero intero").And.Contain(valore);
    }
}
