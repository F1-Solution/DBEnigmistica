using DBEnigmistica.Domain.Catalogo;
using DBEnigmistica.Domain.Diritti;
using FluentAssertions;

namespace DBEnigmistica.Tests.Unit.Domain;

public class CatalogoTests
{
    private static DateTime Utc(int y, int m, int d) => new(y, m, d, 0, 0, 0, DateTimeKind.Utc);

    private static GiocoOriginale NuovoGioco(Origine origine = Origine.Originale) =>
        GiocoOriginale.Crea("cruciverba-classico", 1, origine, Utc(2026, 1, 1));

    [Fact]
    public void StatoInedito_starts_unpublished_and_publishing_returns_a_new_state()
    {
        var prima = new PrimaPubblicazione(Utc(2026, 5, 1), "Sito pubblico");
        var pubblicato = StatoInedito.Inedito.Pubblica(prima);

        StatoInedito.Inedito.EInedito.Should().BeTrue();
        StatoInedito.Inedito.Prima.Should().BeNull();
        pubblicato.EInedito.Should().BeFalse();
        pubblicato.Prima.Should().Be(prima);
    }

    [Fact]
    public void StatoInedito_cannot_go_back()
    {
        var pubblicato = StatoInedito.Pubblicato(new PrimaPubblicazione(Utc(2026, 5, 1), "Sito pubblico"));

        var act = () => pubblicato.Pubblica(new PrimaPubblicazione(Utc(2026, 6, 1), "Altrove"));

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*già stato pubblicato*Sito pubblico*non torna indietro*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void PrimaPubblicazione_requires_a_place(string luogo)
    {
        var act = () => new PrimaPubblicazione(Utc(2026, 1, 1), luogo);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void PrimaPubblicazione_rejects_a_place_longer_than_300_and_non_utc_dates()
    {
        var long301 = new string('x', 301);
        ((Action)(() => new PrimaPubblicazione(Utc(2026, 1, 1), long301))).Should().Throw<ArgumentException>();
        ((Action)(() => new PrimaPubblicazione(new DateTime(2026, 1, 1), "x"))).Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("", 2000, "m")]
    [InlineData("a", 2000, "")]
    [InlineData("a", -3001, "m")]
    [InlineData("a", 9999, "m")]
    public void ProvaPubblicoDominio_validates_its_fields(string autore, int anno, string motivo)
    {
        var act = () => new ProvaPubblicoDominio(autore, anno, motivo);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ProvaPubblicoDominio_rejects_a_reason_longer_than_1000()
    {
        var act = () => new ProvaPubblicoDominio("a", 1900, new string('x', 1001));
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ProvaPubblicoDominio_accepts_boundary_years()
    {
        new ProvaPubblicoDominio("Anonimo", -3000, "Antico").Anno.Should().Be(-3000);
        new ProvaPubblicoDominio("Anonimo", DateTime.UtcNow.Year, "Oggi").Anno.Should().Be(DateTime.UtcNow.Year);
    }

    [Fact]
    public void Soggetto_is_created_with_valid_data()
    {
        var s = Soggetto.Crea(TipoSoggetto.Persona, "Mario Rossi", "RSSMRA80A01H501U", null, Utc(2026, 1, 1));
        s.Id.Should().NotBe(Guid.Empty);
        s.Denominazione.Should().Be("Mario Rossi");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Soggetto_requires_a_name(string denominazione)
    {
        var act = () => Soggetto.Crea(TipoSoggetto.Persona, denominazione, null, null, Utc(2026, 1, 1));
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Soggetto_rejects_oversized_fields_undefined_type_and_non_utc_dates()
    {
        ((Action)(() => Soggetto.Crea(TipoSoggetto.Persona, new string('x', 201), null, null, Utc(2026, 1, 1))))
            .Should().Throw<ArgumentException>();
        ((Action)(() => Soggetto.Crea(TipoSoggetto.Persona, "x", new string('x', 33), null, Utc(2026, 1, 1))))
            .Should().Throw<ArgumentException>();
        ((Action)(() => Soggetto.Crea((TipoSoggetto)0, "x", null, null, Utc(2026, 1, 1))))
            .Should().Throw<ArgumentException>();
        ((Action)(() => Soggetto.Crea(TipoSoggetto.Organizzazione, "x", null, null, new DateTime(2026, 1, 1))))
            .Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Game_created_as_original_starts_unpublished_without_proofs()
    {
        var g = NuovoGioco();
        g.StatoInedito.EInedito.Should().BeTrue();
        g.TitolareId.Should().BeNull();
        g.ProvaPubblicoDominio.Should().BeNull();
        g.Licenze.Should().BeEmpty();
        g.Cessioni.Should().BeEmpty();
    }

    [Fact]
    public void Origine_Rivista_is_rejected_at_creation()
    {
        var act = () => NuovoGioco(Origine.Rivista);
        act.Should().Throw<ArgumentException>().WithMessage("*Rivista*");
    }

    [Fact]
    public void Undefined_origine_is_rejected_at_creation()
    {
        var act = () => NuovoGioco(default);
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("Cruciverba")]
    [InlineData("a")]
    [InlineData("1abc")]
    public void Tipologia_must_match_the_descriptor_format(string tipologia)
    {
        var act = () => GiocoOriginale.Crea(tipologia, 1, Origine.Originale, Utc(2026, 1, 1));
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Version_title_and_date_are_validated()
    {
        ((Action)(() => GiocoOriginale.Crea("sudoku", 0, Origine.Originale, Utc(2026, 1, 1))))
            .Should().Throw<ArgumentException>();
        ((Action)(() => GiocoOriginale.Crea("sudoku", 1, Origine.Originale, Utc(2026, 1, 1), new string('x', 201))))
            .Should().Throw<ArgumentException>();
        ((Action)(() => GiocoOriginale.Crea("sudoku", 1, Origine.Originale, new DateTime(2026, 1, 1))))
            .Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Owner_and_proof_can_be_set_later()
    {
        var titolare = Guid.CreateVersion7();
        var g = NuovoGioco(Origine.PubblicoDominio);
        g.AssegnaTitolare(titolare);
        g.RegistraProvaPubblicoDominio(new ProvaPubblicoDominio("Dante", 1321, "Autore morto da secoli"));

        g.TitolareId.Should().Be(titolare);
        g.ProvaPubblicoDominio!.Autore.Should().Be("Dante");
    }

    [Fact]
    public void Public_domain_proof_is_only_allowed_for_public_domain_games()
    {
        var act = () => NuovoGioco().RegistraProvaPubblicoDominio(new ProvaPubblicoDominio("a", 1900, "m"));
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Empty_owner_id_is_rejected()
    {
        var act = () => NuovoGioco().AssegnaTitolare(Guid.Empty);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void License_is_added_with_licensor_terms_and_period()
    {
        var g = NuovoGioco(Origine.Licenza);
        var licenziante = Guid.CreateVersion7();
        g.AggiungiLicenza(licenziante, "Contratto 12/2026", new Periodo(Utc(2026, 1, 1), null));

        var l = g.Licenze.Should().ContainSingle().Subject;
        l.GiocoId.Should().Be(g.Id);
        l.LicenzianteId.Should().Be(licenziante);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void License_requires_agreement_details(string estremi)
    {
        var act = () => NuovoGioco(Origine.Licenza)
            .AggiungiLicenza(Guid.CreateVersion7(), estremi, new Periodo(Utc(2026, 1, 1), null));
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void License_rejects_oversized_details_and_empty_licensor()
    {
        var g = NuovoGioco(Origine.Licenza);
        var periodo = new Periodo(Utc(2026, 1, 1), null);
        ((Action)(() => g.AggiungiLicenza(Guid.CreateVersion7(), new string('x', 501), periodo)))
            .Should().Throw<ArgumentException>();
        ((Action)(() => g.AggiungiLicenza(Guid.Empty, "x", periodo))).Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Overlapping_exclusive_assignments_are_rejected_immediately()
    {
        var g = NuovoGioco();
        g.AggiungiCessione(Guid.CreateVersion7(), Utc(2026, 1, 1), new Periodo(Utc(2026, 1, 1), Utc(2026, 6, 1)),
            true, Territorio.Paese("IT"), false);

        var act = () => g.AggiungiCessione(Guid.CreateVersion7(), Utc(2026, 2, 1),
            new Periodo(Utc(2026, 3, 1), Utc(2026, 4, 1)), true, Territorio.Mondo, false);

        act.Should().Throw<InvalidOperationException>().WithMessage("*esclusiva*");
        g.Cessioni.Should().HaveCount(1);
    }

    [Fact]
    public void Non_overlapping_or_non_exclusive_assignments_are_accepted()
    {
        var g = NuovoGioco();
        var cliente = Guid.CreateVersion7();
        g.AggiungiCessione(cliente, Utc(2026, 1, 1), new Periodo(Utc(2026, 1, 1), Utc(2026, 6, 1)),
            true, Territorio.Paese("IT"), false);
        g.AggiungiCessione(cliente, Utc(2026, 1, 1), new Periodo(Utc(2026, 1, 1), Utc(2026, 6, 1)),
            true, Territorio.Paese("FR"), false);
        g.AggiungiCessione(cliente, Utc(2026, 1, 1), new Periodo(Utc(2026, 6, 1), null),
            true, Territorio.Paese("IT"), false);
        g.AggiungiCessione(cliente, Utc(2026, 1, 1), new Periodo(Utc(2026, 1, 1), null),
            false, Territorio.Mondo, true);

        g.Cessioni.Should().HaveCount(4);
        g.Cessioni.Last().ComeInedito.Should().BeTrue();
    }

    [Fact]
    public void Assignment_rejects_an_empty_client()
    {
        var act = () => NuovoGioco().AggiungiCessione(Guid.Empty, Utc(2026, 1, 1),
            new Periodo(Utc(2026, 1, 1), null), false, Territorio.Mondo, false);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Assignment_rejects_a_non_utc_signing_date()
    {
        var act = () => NuovoGioco().AggiungiCessione(Guid.CreateVersion7(), new DateTime(2026, 1, 1),
            new Periodo(Utc(2026, 1, 1), null), false, Territorio.Mondo, false);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void First_publication_is_recorded_once()
    {
        var g = NuovoGioco();
        g.RegistraPrimaPubblicazione(new PrimaPubblicazione(Utc(2026, 5, 1), "Sito pubblico"));
        g.StatoInedito.EInedito.Should().BeFalse();

        var act = () => g.RegistraPrimaPubblicazione(new PrimaPubblicazione(Utc(2026, 6, 1), "Altro"));
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void A_game_can_be_saved_without_proofs_and_policy_candidate_reflects_its_state()
    {
        var g = NuovoGioco(Origine.Licenza);
        var c = g.ComeCandidato(Utc(2026, 7, 1), true, Territorio.Mondo);
        c.Origine.Should().Be(Origine.Licenza);
        c.Licenze.Should().BeEmpty();
        c.Approvato.Should().BeTrue();
        c.Istante.Should().Be(Utc(2026, 7, 1));
    }
}
