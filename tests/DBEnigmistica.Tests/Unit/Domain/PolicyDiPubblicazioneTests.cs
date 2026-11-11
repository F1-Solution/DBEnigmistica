using DBEnigmistica.Domain.Catalogo;
using DBEnigmistica.Domain.Diritti;
using DBEnigmistica.Domain.Pubblicazione;
using FluentAssertions;

namespace DBEnigmistica.Tests.Unit.Domain;

public class PolicyDiPubblicazioneTests
{
    private static readonly DateTime Adesso = new(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc);

    private static DateTime Utc(int y, int m, int d) => new(y, m, d, 0, 0, 0, DateTimeKind.Utc);

    private static readonly Periodo Aperto = new(Utc(2026, 1, 1), null);

    private static LicenzaInEntrata Licenza(Periodo p) =>
        LicenzaInEntrata.Crea(Guid.CreateVersion7(), Guid.CreateVersion7(), "Accordo", p);

    private static Cessione Cess(Periodo p, bool esclusiva, string territorio, bool comeInedito) =>
        Cessione.Crea(Guid.CreateVersion7(), Guid.CreateVersion7(), Utc(2026, 1, 1), p, esclusiva,
            Territorio.Da(territorio), comeInedito);

    private static CandidatoPubblicazione Valido(
        Origine origine = Origine.Originale,
        IReadOnlyList<LicenzaInEntrata>? licenze = null,
        ProvaPubblicoDominio? prova = null,
        IReadOnlyList<Cessione>? cessioni = null,
        StatoInedito? stato = null,
        bool approvato = true) => new(
            origine,
            licenze ?? [],
            prova,
            cessioni ?? [],
            stato ?? StatoInedito.Inedito,
            approvato,
            Adesso,
            Territorio.Mondo);

    private static object[] Caso(string nome, CandidatoPubblicazione c, MotivoBlocco[] attesi) =>
        [nome, c, attesi];

    public static IEnumerable<object[]> Casi()
    {
        var prova = new ProvaPubblicoDominio("Dante", 1321, "Morto da secoli");
        var licenzaValida = Licenza(Aperto);

        yield return Caso("originale valido", Valido(), []);
        yield return Caso("licenza valida", Valido(Origine.Licenza, licenze: [licenzaValida]), []);
        yield return Caso("pubblico dominio valido", Valido(Origine.PubblicoDominio, prova: prova), []);
        yield return Caso("rivista", Valido(Origine.Rivista), [MotivoBlocco.OrigineRivista]);
        yield return Caso("origine non definita (default)", Valido(default), [MotivoBlocco.OrigineNonValida]);
        yield return Caso("origine fuori dall'enum", Valido((Origine)99), [MotivoBlocco.OrigineNonValida]);
        yield return Caso("origine non valida e non approvato", Valido((Origine)(-1), approvato: false),
            [MotivoBlocco.OrigineNonValida, MotivoBlocco.NonApprovato]);
        yield return Caso("licenza senza licenze", Valido(Origine.Licenza), [MotivoBlocco.ProvaLicenzaMancante]);
        yield return Caso("licenza scaduta",
            Valido(Origine.Licenza, licenze: [Licenza(new Periodo(Utc(2025, 1, 1), Utc(2026, 1, 1)))]),
            [MotivoBlocco.LicenzaNonValidaAllIstante]);
        yield return Caso("licenza non ancora iniziata",
            Valido(Origine.Licenza, licenze: [Licenza(new Periodo(Utc(2027, 1, 1), null))]),
            [MotivoBlocco.LicenzaNonValidaAllIstante]);
        yield return Caso("licenza che inizia ora",
            Valido(Origine.Licenza, licenze: [Licenza(new Periodo(Adesso, null))]), []);
        yield return Caso("licenza che finisce ora",
            Valido(Origine.Licenza, licenze: [Licenza(new Periodo(Utc(2026, 1, 1), Adesso))]),
            [MotivoBlocco.LicenzaNonValidaAllIstante]);
        yield return Caso("una licenza scaduta e una valida",
            Valido(Origine.Licenza,
                licenze: [Licenza(new Periodo(Utc(2025, 1, 1), Utc(2026, 1, 1))), licenzaValida]), []);
        yield return Caso("pubblico dominio senza prova", Valido(Origine.PubblicoDominio),
            [MotivoBlocco.ProvaPubblicoDominioMancante]);
        yield return Caso("cessione esclusiva attiva",
            Valido(cessioni: [Cess(Aperto, true, "IT", false)]), [MotivoBlocco.CessioneEsclusivaInConflitto]);
        yield return Caso("cessione esclusiva che inizia ora",
            Valido(cessioni: [Cess(new Periodo(Adesso, null), true, "FR", false)]),
            [MotivoBlocco.CessioneEsclusivaInConflitto]);
        yield return Caso("cessione esclusiva che finisce ora",
            Valido(cessioni: [Cess(new Periodo(Utc(2026, 1, 1), Adesso), true, "MONDO", false)]), []);
        yield return Caso("cessione esclusiva futura",
            Valido(cessioni: [Cess(new Periodo(Utc(2027, 1, 1), null), true, "MONDO", false)]), []);
        yield return Caso("cessione non esclusiva attiva",
            Valido(cessioni: [Cess(Aperto, false, "MONDO", false)]), []);
        yield return Caso("inedito promesso come inedito",
            Valido(cessioni: [Cess(Aperto, false, "IT", true)]), [MotivoBlocco.PromessoComeInedito]);
        yield return Caso("inedito con cessione come inedito futura",
            Valido(cessioni: [Cess(new Periodo(Utc(2027, 1, 1), null), false, "IT", true)]),
            [MotivoBlocco.PromessoComeInedito]);
        yield return Caso("inedito con cessione come inedito terminata",
            Valido(cessioni: [Cess(new Periodo(Utc(2026, 1, 1), Utc(2026, 6, 1)), false, "IT", true)]), []);
        yield return Caso("gia pubblicato con cessione come inedito attiva",
            Valido(stato: StatoInedito.Pubblicato(new PrimaPubblicazione(Utc(2026, 3, 1), "Rivista X")),
                cessioni: [Cess(Aperto, false, "IT", true)]), []);
        yield return Caso("non approvato", Valido(approvato: false), [MotivoBlocco.NonApprovato]);
        yield return Caso("tutti i motivi insieme",
            Valido(Origine.Rivista, approvato: false, cessioni: [Cess(Aperto, true, "MONDO", true)]),
            [MotivoBlocco.OrigineRivista, MotivoBlocco.CessioneEsclusivaInConflitto,
             MotivoBlocco.PromessoComeInedito, MotivoBlocco.NonApprovato]);
        yield return Caso("licenza senza prove e non approvata",
            Valido(Origine.Licenza, approvato: false),
            [MotivoBlocco.ProvaLicenzaMancante, MotivoBlocco.NonApprovato]);
    }

    [Theory]
    [MemberData(nameof(Casi))]
    public void Valuta_returns_all_blocking_reasons(string nome, CandidatoPubblicazione c, MotivoBlocco[] attesi)
    {
        var esito = PolicyDiPubblicazione.Valuta(c);

        esito.Motivi.Should().Equal(attesi, nome);
        esito.Pubblicabile.Should().Be(attesi.Length == 0, nome);
    }

    [Fact]
    public void Magazine_origin_is_never_publishable_even_with_everything_else_valid()
    {
        var esito = PolicyDiPubblicazione.Valuta(Valido(Origine.Rivista, approvato: true));
        esito.Pubblicabile.Should().BeFalse();
        esito.Motivi.Should().Equal(MotivoBlocco.OrigineRivista);
    }

    [Fact]
    public void Exclusive_assignment_in_a_country_does_not_block_a_publication_in_another_country()
    {
        var c = Valido(cessioni: [Cess(Aperto, true, "IT", false)])
            with
        { TerritorioDiPubblicazione = Territorio.Paese("FR") };
        PolicyDiPubblicazione.Valuta(c).Pubblicabile.Should().BeTrue();
    }

    [Fact]
    public void Original_and_license_games_without_an_owner_are_publishable()
    {
        var licenza = Valido(Origine.Licenza, licenze: [Licenza(Aperto)]);
        PolicyDiPubblicazione.Valuta(licenza).Pubblicabile.Should().BeTrue();
        PolicyDiPubblicazione.Valuta(Valido()).Pubblicabile.Should().BeTrue();
    }
}
