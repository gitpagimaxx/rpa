using Dapper;

namespace Rpa.Infrastructure.Persistence;

public sealed class DatabaseInitializer(IDbConnectionFactory factory)
{
    public async Task InitializeAsync(CancellationToken ct)
    {
        using var conn = factory.Create();

        const string sql = """
        create table if not exists exchange_rates (
            id text primary key,
            base_currency text not null,
            quote_currency text not null,
            rate real not null,
            collected_at_utc text not null,
            source text not null
        );

        create index if not exists ix_exchange_rates_pair_time
            on exchange_rates (base_currency, quote_currency, collected_at_utc desc);
        """;

        await conn.ExecuteAsync(new CommandDefinition(sql, cancellationToken: ct));
    }
}