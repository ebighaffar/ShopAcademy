namespace Shop.Infrastructure.Caching.Redis;

/// <summary>
/// Bound from the "Redis" section of configuration. Holds the connection string,
/// an optional key prefix (namespacing this app's keys within a shared instance),
/// and a default entry lifetime.
/// </summary>
public class RedisSettings
{
    public const string SectionName = "Redis";

    public string ConnectionString { get; set; } = "localhost:6379";
    public string InstanceName { get; set; } = "Shop:";
    public int DefaultExpirationMinutes { get; set; } = 10;
}
