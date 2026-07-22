namespace Shop.Infrastructure.Persistence.Mongo;

/// <summary>
/// Bound from the "Mongo" section of configuration. Holds the connection string,
/// database name, and collection names used by the MongoDB persistence layer.
/// </summary>
public class MongoSettings
{
    public const string SectionName = "Mongo";

    public string ConnectionString { get; set; } = "mongodb://localhost:27017";
    public string Database { get; set; } = "ShopDb";
    public string ExceptionLogsCollection { get; set; } = "exception_logs";
}
