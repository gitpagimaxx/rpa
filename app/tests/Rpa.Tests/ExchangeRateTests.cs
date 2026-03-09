using FluentAssertions;
using Rpa.Domain.Exchange;

namespace Rpa.Tests;

public sealed class ExchangeRateTests
{
    [Fact]
    public void Create_ShouldThrow_WhenRateIsZeroOrNegative()
    {
        var act = () => ExchangeRate.Create(
            Currency.USD, Currency.BRL, 0m, DateTimeOffset.UtcNow, DataSource.From("wise.com"));

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Create_ShouldCreateValidEntity()
    {
        var now = new DateTimeOffset(2026, 03, 08, 12, 00, 00, TimeSpan.Zero);

        var e = ExchangeRate.Create(Currency.USD, Currency.BRL, 5.25m, now, DataSource.From("wise.com"));

        e.Id.Should().NotBeEmpty();
        e.BaseCurrency.Code.Should().Be("USD");
        e.QuoteCurrency.Code.Should().Be("BRL");
        e.Rate.Should().Be(5.25m);
        e.CollectedAtUtc.Should().Be(now);
        e.Source.Name.Should().Be("wise.com");
    }
}