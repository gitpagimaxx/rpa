using Dapper;
using Rpa.Domain.Abstractions;
using Rpa.Domain.Exchange;

namespace Rpa.Infrastructure.Persistence;

public sealed class ExchangeRateRepository(IDbConnectionFactory factory) : IExchangeRateRepository
{
    public async Task AddAsync(ExchangeRate rate, CancellationToken ct)
    {
        const string sql = """
        insert into exchange_rates (id, base_currency, quote_currency, rate, collected_at_utc, source)
        values (@Id, @BaseCurrency, @QuoteCurrency, @Rate, @CollectedAtUtc, @Source);
        """;

        using var conn = factory.Create();

        var args = new
        {
            rate.Id,
            BaseCurrency = rate.BaseCurrency.Code,
            QuoteCurrency = rate.QuoteCurrency.Code,
            rate.Rate,
            rate.CollectedAtUtc,
            Source = rate.Source.Name
        };

        await conn.ExecuteAsync(new CommandDefinition(sql, args, cancellationToken: ct));
    }
}