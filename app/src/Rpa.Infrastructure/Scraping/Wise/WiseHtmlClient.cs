using Microsoft.Extensions.Options;
using Rpa.Domain.Abstractions;

namespace Rpa.Infrastructure.Scraping.Wise;

public sealed class WiseHtmlClient(IHttpClientFactory factory, IOptions<WiseOptions> options) : IExchangeRateHtmlClient
{
    public async Task<string> GetUsdBrlPageHtmlAsync(CancellationToken ct)
    {
        var client = factory.CreateClient("wise");
        client.DefaultRequestHeaders.UserAgent.ParseAdd(options.Value.UserAgent);
        return await client.GetStringAsync(options.Value.Url, ct);
    }
}