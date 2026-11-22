using DBEnigmistica.Application.Valutazione;
using FluentAssertions;

namespace DBEnigmistica.Tests.Unit.Valutazione;

public class AbbinamentoGiochiTests
{
    private static GiocoPagina Gioco(string chiave, double x, double y = 0.1, double l = 0.4, double a = 0.2) =>
        new(chiave, new RiquadroGioco(x, y, l, a));

    [Fact]
    public void Abbina_RiquadriUguali_UnaCoppia()
    {
        var esito = AbbinamentoGiochi.Abbina([Gioco("nota:anagramma", 0.1)], [Gioco("nota:anagramma", 0.1)]);

        esito.Coppie.Should().Equal([(0, 0)]);
        esito.Mancanti.Should().BeEmpty();
        esito.InPiu.Should().BeEmpty();
    }

    [Fact]
    public void Abbina_GiocoMancante_EAttesoSenzaCoppia()
    {
        var esito = AbbinamentoGiochi.Abbina([Gioco("nota:anagramma", 0.1), Gioco("nota:sciarada", 0.6)], [Gioco("nota:anagramma", 0.1)]);

        esito.Mancanti.Should().Equal([1]);
        esito.InPiu.Should().BeEmpty();
    }

    [Fact]
    public void Abbina_GiocoInPiu_EEstrattoSenzaCoppia()
    {
        var esito = AbbinamentoGiochi.Abbina([Gioco("nota:anagramma", 0.1)], [Gioco("nota:anagramma", 0.1), Gioco("nota:sciarada", 0.6)]);

        esito.InPiu.Should().Equal([1]);
        esito.Mancanti.Should().BeEmpty();
    }

    [Fact]
    public void Abbina_TipologiaSbagliata_NonAbbinaConLaChiaveMaAbbinaSenza()
    {
        var attesi = new[] { Gioco("nota:anagramma", 0.1) };
        var estratti = new[] { Gioco("nota:sciarada", 0.1) };

        AbbinamentoGiochi.Abbina(attesi, estratti).Coppie.Should().BeEmpty();
        AbbinamentoGiochi.Abbina(attesi, estratti, conChiave: false).Coppie.Should().Equal([(0, 0)]);
    }

    [Fact]
    public void Abbina_IouSottoSogliaDi05_NonAbbina()
    {
        // Due riquadri larghi 0,4 sfalsati di 0,3: intersezione 0,1 su unione 0,7, IoU circa 0,14.
        var esito = AbbinamentoGiochi.Abbina([Gioco("nota:anagramma", 0.1)], [Gioco("nota:anagramma", 0.4)]);

        esito.Coppie.Should().BeEmpty();
        esito.Mancanti.Should().Equal([0]);
        esito.InPiu.Should().Equal([0]);
    }

    [Fact]
    public void Iou_RiquadriDisgiunti_Zero()
    {
        AbbinamentoGiochi.Iou(new RiquadroGioco(0, 0, 0.1, 0.1), new RiquadroGioco(0.5, 0.5, 0.1, 0.1)).Should().Be(0);
    }

    [Fact]
    public void Abbina_Goloso_PrendeLaCoppiaConIouPiuAlto()
    {
        var attesi = new[] { Gioco("proposta", 0.10) };
        var estratti = new[] { Gioco("proposta", 0.16), Gioco("proposta", 0.11) };

        AbbinamentoGiochi.Abbina(attesi, estratti).Coppie.Should().Equal([(0, 1)]);
    }
}
