using System.Data;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;

namespace Rpa.Infrastructure.Persistence;

public sealed class SqliteConnectionFactory(IOptions<SqliteOptions> options) : IDbConnectionFactory
{
    public IDbConnection Create() => new SqliteConnection(options.Value.ConnectionString);
}