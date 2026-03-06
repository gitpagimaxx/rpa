using AngleSharp;
using Rpa.Domain.Abstractions;
using Rpa.Infrastructure.Scraping.Parsing;

namespace Rpa.Infrastructure.Scraping.Wise;

public sealed class WiseUsdBrlHtmlParser : IUsdBrlHtmlParser
{
    public bool TryParseUsdBrl(string html, out decimal rate)
    {
        var context = BrowsingContext.New(Configuration.Default);
        var doc = context.OpenAsync(req => req.Content(html)).GetAwaiter().GetResult();

        var text = doc.Body?.TextContent ?? doc.DocumentElement.TextContent;
        var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (var line in lines.Where(l =>
                     l.Contains("USD", StringComparison.OrdinalIgnoreCase) &&
                     l.Contains("BRL", StringComparison.OrdinalIgnoreCase)))
        {
            if (DecimalParsing.TryExtractDecimal(line, out var parsed))
            {
                rate = parsed;
                return true;
            }
        }

        if (DecimalParsing.TryExtractDecimal(text, out var fallback))
        {
            rate = fallback;
            return true;
        }

        rate = 0m;
        return false;
    }
}