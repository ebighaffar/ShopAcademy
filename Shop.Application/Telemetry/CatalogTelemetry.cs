using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Shop.Application.Telemetry;

/// <summary>
/// Custom spans and metrics for products and their cache. <see cref="Name"/> must be registered
/// with AddSource and AddMeter, or everything here is silently dropped.
/// </summary>
public static class CatalogTelemetry
{
    public const string Name = "ShopAPI.Catalog";

    public const string OperationCreated = "created";
    public const string OperationUpdated = "updated";
    public const string OperationDeleted = "deleted";

    public const string ProductIdTag = "product.id";

    public static readonly ActivitySource Source = new(Name);

    private static readonly Meter Meter = new(Name);

    private static readonly Counter<long> ProductChanges = Meter.CreateCounter<long>(
        "shop.products.changes",
        description: "Product writes by operation.");

    private static readonly Counter<long> CacheRequests = Meter.CreateCounter<long>(
        "shop.cache.requests",
        description: "Product cache lookups by result.");

    public static void RecordProductChange(string operation)
        => ProductChanges.Add(1, new KeyValuePair<string, object?>("operation", operation));

    public static void RecordCacheHit()
        => CacheRequests.Add(1, new KeyValuePair<string, object?>("result", "hit"));

    public static void RecordCacheMiss()
        => CacheRequests.Add(1, new KeyValuePair<string, object?>("result", "miss"));
}
