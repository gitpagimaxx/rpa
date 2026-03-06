using System.Data;

namespace Rpa.Infrastructure.Persistence;

public interface IDbConnectionFactory
{
    IDbConnection Create();
}