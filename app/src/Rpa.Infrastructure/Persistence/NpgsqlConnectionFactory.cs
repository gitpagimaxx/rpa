using System.Data;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Rpa.Infrastructure.Persistence;

public sealed class NpgsqlConnectionFactory(IOptions<PostgresOptions> options) : IDbConnectionFactory
{
    public IDbConnection Create() => new NpgsqlConnection(options.Value.ConnectionString);
}