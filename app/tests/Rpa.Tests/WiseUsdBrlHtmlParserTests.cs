using FluentAssertions;

using Rpa.Infrastructure.Scraping.Wise;

namespace Rpa.Tests;

public sealed class WiseUsdBrlHtmlParserTests
{
    [Fact]
    public void TryParseUsdBrl_ShouldParse_FromPrimarySelector()
    {
        var html = """
        <html>
          <body>
            <div class="_midMarketRateAmount_14arr_139">
              <span dir="ltr">1 USD = 5,253 BRL</span>
            </div>
          </body>
        </html>
        """;

        var parser = new WiseUsdBrlHtmlParser();

        var ok = parser.TryParseUsdBrl(html, out var rate);

        ok.Should().BeTrue();
        rate.Should().Be(5.253m);
    }

    [Fact]
    public void TryParseUsdBrl_ShouldParse_FromFallbackSpanDir()
    {
        var html = """
        <html>
          <body>
            <span dir="ltr">algo</span>
            <span dir="ltr">1 USD = 5,111 BRL</span>
          </body>
        </html>
        """;

        var parser = new WiseUsdBrlHtmlParser();

        var ok = parser.TryParseUsdBrl(html, out var rate);

        ok.Should().BeTrue();
        rate.Should().Be(5.111m);
    }

    [Fact]
    public void TryParseUsdBrl_ShouldReturnFalse_WhenNoRateFound()
    {
        var html = "<html><body><div>sem dados</div></body></html>";
        var parser = new WiseUsdBrlHtmlParser();

        var ok = parser.TryParseUsdBrl(html, out var rate);

        ok.Should().BeFalse();
        rate.Should().Be(0m);
    }
}