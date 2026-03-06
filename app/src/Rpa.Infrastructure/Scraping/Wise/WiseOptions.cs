namespace Rpa.Infrastructure.Scraping.Wise;

public sealed class WiseOptions
{
    public const string SectionName = "Wise";

    public string Url { get; init; } = "https://wise.com/br/currency-converter/dolar-hoje";

    public string UserAgent { get; init; } =
        "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/122.0.0.0 Safari/537.36";
}