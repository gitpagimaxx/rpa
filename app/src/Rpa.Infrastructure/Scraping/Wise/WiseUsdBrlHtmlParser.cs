using AngleSharp;
using AngleSharp.Dom;
using Rpa.Domain.Abstractions;
using System.Text.RegularExpressions;

namespace Rpa.Infrastructure.Scraping.Wise;

public sealed partial class WiseUsdBrlHtmlParser : IUsdBrlHtmlParser
{
    private static readonly Regex _usdBrlRegex = new(
        @"1\s*USD\s*=\s*(?<rate>\d{1,3}([.,]\d{1,6})?)\s*BRL",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static Regex UsdBrlRegex() => _usdBrlRegex;

    public bool TryParseUsdBrl(string html, out decimal rate)
    {
        rate = 0m;

        if (string.IsNullOrWhiteSpace(html))
            return false;

        var doc = ParseHtml(html);

        // 1) Primary selector (mais estável com base no trecho que você trouxe)
        // Usa "contains" no atributo class para não acoplar no sufixo "_14arr_139"
        var primary = doc.QuerySelector("""div[class*="_midMarketRateAmount_"] span[dir="ltr"]""")?.TextContent;
        if (TryExtractRate(primary, out rate))
            return true;

        // 2) Fallback: qualquer span dir=ltr que contenha USD e BRL
        foreach (var span in doc.QuerySelectorAll("""span[dir="ltr"]"""))
        {
            var text = span.TextContent;
            if (!text.Contains("USD", StringComparison.OrdinalIgnoreCase) ||
                !text.Contains("BRL", StringComparison.OrdinalIgnoreCase))
                continue;

            if (TryExtractRate(text, out rate))
                return true;
        }

        // 3) Último fallback: corpo todo (menos ideal, mas evita quebra total)
        var bodyText = doc.Body?.TextContent;
        return TryExtractRate(bodyText, out rate);
    }

    private static IDocument ParseHtml(string html)
    {
        // AngleSharp: parsing rápido e robusto
        var context = BrowsingContext.New(Configuration.Default);
        return context.OpenAsync(req => req.Content(html)).GetAwaiter().GetResult();
    }

    private static bool TryExtractRate(string? text, out decimal rate)
    {
        rate = 0m;
        if (string.IsNullOrWhiteSpace(text)) return false;

        var m = UsdBrlRegex().Match(text);
        if (!m.Success) return false;

        var raw = m.Groups["rate"].Value.Trim();

        // Normaliza pt-BR: "5,253" -> "5.253"
        raw = raw.Replace(",", ".");

        return decimal.TryParse(raw, System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out rate);
    }
}