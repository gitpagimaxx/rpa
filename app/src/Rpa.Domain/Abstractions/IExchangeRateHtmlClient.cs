namespace Rpa.Domain.Abstractions;

public interface IExchangeRateHtmlClient
{
    Task<string> GetUsdBrlPageHtmlAsync(CancellationToken ct);
}