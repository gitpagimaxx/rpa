namespace Rpa.Infrastructure.Persistence;

public sealed class SqliteOptions
{
    public const string SectionName = "Sqlite";
    public string ConnectionString { get; init; } = default!;
}