namespace Rpa.Domain.Abstractions;

public interface IUsdBrlHtmlParser
{
    bool TryParseUsdBrl(string html, out decimal rate);
}