using Dapper;

namespace Rpa.Infrastructure.Persistence;

public sealed class DatabaseInitializer(IDbConnectionFactory factory)
{
    public async Task InitializeAsync(CancellationToken ct)
    {
        using var conn = factory.Create();

        // idempotente
        const string sql = """
        create table if not exists exchange_rates (
            id uuid primary key,
            base_currency varchar(8) not null,
            quote_currency varchar(8) not null,
            rate numeric(18,6) not null,
            collected_at_utc timestamptz not null,
            source varchar(64) not null
        );

        create index if not exists ix_exchange_rates_pair_time
            on exchange_rates (base_currency, quote_currency, collected_at_utc desc);
        """;

        await conn.ExecuteAsync(new CommandDefinition(sql, cancellationToken: ct));
    }
}