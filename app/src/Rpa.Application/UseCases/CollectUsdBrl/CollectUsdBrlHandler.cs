using Rpa.Domain.Abstractions;
using Rpa.Domain.Exchange;

namespace Rpa.Application.UseCases.CollectUsdBrl;

public sealed class CollectUsdBrlHandler(
    IExchangeRateHtmlClient htmlClient,
    IUsdBrlHtmlParser parser,
    IExchangeRateRepository repository,
    IClock clock)
{
    // Guardrails contra parsing errado
    private const decimal MinExpected = 0.50m;
    private const decimal MaxExpected = 20.00m;

    public async Task<ExchangeRate> HandleAsync(CancellationToken ct)
    {
        var html = await htmlClient.GetUsdBrlPageHtmlAsync(ct);

        if (!parser.TryParseUsdBrl(html, out var rate))
            throw new InvalidOperationException("Could not parse USD/BRL from HTML.");

        if (rate < MinExpected || rate > MaxExpected)
            throw new InvalidOperationException($"Parsed rate outside expected range: {rate}.");

        var entity = ExchangeRate.Create(
            baseCurrency: Currency.USD,
            quoteCurrency: Currency.BRL,
            rate: rate,
            collectedAtUtc: clock.UtcNow,
            source: DataSource.From("wise.com"));

        await repository.AddAsync(entity, ct);

        return entity;
    }
}