using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace Shop.Infrastructure.Persistence.PostgreSql.Dapper;

public class SqlConnectionFactory(IConfiguration configuration) : IDbConnectionFactory
{
    private readonly string _connectionString =
        configuration.GetConnectionString("Default")
        ?? throw new InvalidOperationException(
            "Connection string 'Default' is not configured.");

    public IDbConnection CreateConnection() => new SqlConnection(_connectionString);
}