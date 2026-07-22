using System.Data;

namespace Shop.Infrastructure.Persistence.PostgreSql.Dapper;

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}
