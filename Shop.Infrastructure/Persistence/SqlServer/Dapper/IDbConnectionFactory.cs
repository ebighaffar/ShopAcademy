using System.Data;

namespace Shop.Infrastructure.Persistence.SqlServer.Dapper;

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}
