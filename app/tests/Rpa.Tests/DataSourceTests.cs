using FluentAssertions;
using Rpa.Domain.Exchange;

namespace Rpa.Tests;

public sealed class DataSourceTests
{
    [Fact]
    public void From_ShouldTrim()
    {
        var s = DataSource.From("  wise.com  ");
        s.Name.Should().Be("wise.com");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void From_ShouldThrow_WhenEmpty(string? input)
    {
        var act = () => DataSource.From(input!);
        act.Should().Throw<ArgumentException>();
    }
}
