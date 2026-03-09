using FluentAssertions;
using Rpa.Domain.Exchange;

namespace Rpa.Tests;

public sealed class CurrencyTests
{
    [Fact]
    public void From_ShouldUppercaseAndTrim()
    {
        var c = Currency.From(" brl ");
        c.Code.Should().Be("BRL");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void From_ShouldThrow_WhenEmpty(string? input)
    {
        var act = () => Currency.From(input!);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void From_ShouldThrow_WhenTooShort()
    {
        var act = () => Currency.From("AA");
        act.Should().Throw<ArgumentException>();
    }
}