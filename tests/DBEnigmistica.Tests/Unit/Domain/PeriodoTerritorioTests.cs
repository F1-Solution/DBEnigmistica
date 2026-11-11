using DBEnigmistica.Domain.Diritti;
using FluentAssertions;

namespace DBEnigmistica.Tests.Unit.Domain;

public class PeriodoTerritorioTests
{
    private static DateTime Utc(int y, int m, int d) => new(y, m, d, 0, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Periodo_rejects_end_not_after_start()
    {
        var act = () => new Periodo(Utc(2026, 1, 1), Utc(2026, 1, 1));
        act.Should().Throw<ArgumentException>().WithMessage("*fine*");
    }

    [Theory]
    [InlineData(DateTimeKind.Local)]
    [InlineData(DateTimeKind.Unspecified)]
    public void Periodo_rejects_non_utc_dates(DateTimeKind kind)
    {
        var inizio = new DateTime(2026, 1, 1, 0, 0, 0, kind);
        var act = () => new Periodo(inizio, null);
        act.Should().Throw<ArgumentException>().WithMessage("*UTC*");
    }

    [Fact]
    public void Periodo_rejects_non_utc_end()
    {
        var act = () => new Periodo(Utc(2026, 1, 1), new DateTime(2027, 1, 1, 0, 0, 0, DateTimeKind.Unspecified));
        act.Should().Throw<ArgumentException>().WithMessage("*UTC*");
    }

    [Fact]
    public void Periodo_truncates_to_the_microsecond()
    {
        var p = new Periodo(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddTicks(19), null);
        p.Inizio.Ticks.Should().Be(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddTicks(10).Ticks);
    }

    [Fact]
    public void Contiene_includes_start_and_excludes_end()
    {
        var p = new Periodo(Utc(2026, 1, 1), Utc(2026, 2, 1));
        p.Contiene(Utc(2026, 1, 1)).Should().BeTrue();
        p.Contiene(Utc(2026, 1, 31)).Should().BeTrue();
        p.Contiene(Utc(2026, 2, 1)).Should().BeFalse();
        p.Contiene(Utc(2025, 12, 31)).Should().BeFalse();
    }

    [Fact]
    public void Open_ended_period_contains_every_later_instant()
    {
        new Periodo(Utc(2026, 1, 1), null).Contiene(Utc(2999, 1, 1)).Should().BeTrue();
    }

    [Fact]
    public void SiSovrappone_is_true_for_partial_overlap_and_false_for_gaps()
    {
        var a = new Periodo(Utc(2026, 1, 1), Utc(2026, 3, 1));
        a.SiSovrappone(new Periodo(Utc(2026, 2, 1), Utc(2026, 4, 1))).Should().BeTrue();
        a.SiSovrappone(new Periodo(Utc(2026, 4, 1), Utc(2026, 5, 1))).Should().BeFalse();
        new Periodo(Utc(2026, 4, 1), Utc(2026, 5, 1)).SiSovrappone(a).Should().BeFalse();
    }

    [Fact]
    public void SiSovrappone_is_true_for_nested_and_open_ended_periods()
    {
        var open = new Periodo(Utc(2026, 1, 1), null);
        var nested = new Periodo(Utc(2026, 3, 1), Utc(2026, 4, 1));
        open.SiSovrappone(nested).Should().BeTrue();
        nested.SiSovrappone(open).Should().BeTrue();
        new Periodo(Utc(2026, 1, 1), Utc(2026, 2, 1)).SiSovrappone(new Periodo(Utc(2026, 2, 1), Utc(2026, 3, 1)))
            .Should().BeFalse();
        new Periodo(Utc(2026, 1, 1), Utc(2026, 2, 1)).SiSovrappone(new Periodo(Utc(2026, 2, 1), null))
            .Should().BeFalse();
    }

    [Fact]
    public void TerminatoPrima_is_true_once_the_end_is_reached()
    {
        var p = new Periodo(Utc(2026, 1, 1), Utc(2026, 2, 1));
        p.TerminatoPrima(Utc(2026, 2, 1)).Should().BeTrue();
        p.TerminatoPrima(Utc(2026, 1, 31)).Should().BeFalse();
        new Periodo(Utc(2026, 1, 1), null).TerminatoPrima(Utc(2999, 1, 1)).Should().BeFalse();
    }

    [Fact]
    public void Mondo_has_code_MONDO()
    {
        Territorio.Mondo.Codice.Should().Be("MONDO");
    }

    [Theory]
    [InlineData("IT")]
    [InlineData("FR")]
    public void Paese_accepts_two_uppercase_letters(string code)
    {
        Territorio.Paese(code).Codice.Should().Be(code);
    }

    [Theory]
    [InlineData("it")]
    [InlineData("ITA")]
    [InlineData("I1")]
    [InlineData("")]
    [InlineData(null)]
    public void Paese_rejects_anything_else(string? code)
    {
        var act = () => Territorio.Paese(code!);
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("MONDO", "IT", true)]
    [InlineData("IT", "MONDO", true)]
    [InlineData("MONDO", "MONDO", true)]
    [InlineData("IT", "IT", true)]
    [InlineData("IT", "FR", false)]
    public void SiSovrappone_follows_the_territory_rules(string a, string b, bool expected)
    {
        Territorio.Da(a).SiSovrappone(Territorio.Da(b)).Should().Be(expected);
    }

    [Fact]
    public void Da_parses_MONDO_and_country_codes()
    {
        Territorio.Da("MONDO").Should().Be(Territorio.Mondo);
        Territorio.Da("IT").Should().Be(Territorio.Paese("IT"));
    }
}
