using DBEnigmistica.Infrastructure.Services.Pdf;
using FluentAssertions;

namespace DBEnigmistica.Tests.Unit.Pdf;

public class EstrazioneOptionsTests
{
    private static readonly EstrazioneOptionsValidator Validatore = new();

    [Fact]
    public void Dpi_PredefinitoE200()
    {
        new EstrazioneOptions().Dpi.Should().Be(200);
    }

    [Theory]
    [InlineData(150)]
    [InlineData(200)]
    [InlineData(300)]
    public void Dpi_NellIntervallo_EValido(int dpi)
    {
        Validatore.Validate(null, new EstrazioneOptions { Dpi = dpi }).Succeeded.Should().BeTrue();
    }

    [Theory]
    [InlineData(149)]
    [InlineData(301)]
    [InlineData(0)]
    public void Dpi_FuoriIntervallo_ChiedeDiImpostareEstrazioneDpi(int dpi)
    {
        var esito = Validatore.Validate(null, new EstrazioneOptions { Dpi = dpi });

        esito.Failed.Should().BeTrue();
        esito.Failures.Should().ContainSingle().Which.Should().Contain("Estrazione:Dpi").And.Contain("150").And.Contain("300");
    }
}
