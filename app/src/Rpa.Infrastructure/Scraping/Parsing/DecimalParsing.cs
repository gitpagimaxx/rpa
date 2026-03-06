using System.Globalization;
using System.Text.RegularExpressions;

namespace Rpa.Infrastructure.Scraping.Parsing;

public static partial class DecimalParsing
{
    [GeneratedRegex(@"(?<!\d)(\d{1,3}([.,]\d{1,6})?)(?!\d)", RegexOptions.Compiled)]
    private static partial Regex NumberRegex();

    public static bool TryExtractDecimal(string input, out decimal value)
    {
        var m = NumberRegex().Match(input);
        if (!m.Success)
        {
            value = 0m;
            return false;
        }

        var raw = m.Groups[1].Value.Trim().Replace(" ", "").Replace(",", ".");
        return decimal.TryParse(raw, NumberStyles.Number, CultureInfo.InvariantCulture, out value);
    }
}