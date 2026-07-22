using System.Reflection;
using System.Runtime.CompilerServices;
using Dapper;
using Shop.Application.Repository;
using Shop.Domain.Entities;
using Shop.Domain.Enums;
using Shop.Domain.ValueObjects;
using Shop.Infrastructure.Persistence.PostgreSql.EfCore.Repositories;

namespace Shop.Infrastructure.Persistence.PostgreSql.Dapper.Repositories;

/// <summary>
/// Dapper-based alternative to <see cref="ProductRepository"/>.
/// EF Core has a change tracker that defers writes until SaveChanges; Dapper has none, so Add/Update/Remove
/// here stage entities in-memory and SaveChangesAsync flushes them in one batch against a single connection,
/// to keep the same IRepository contract semantics as the EF implementation.
/// </summary>
public class DapperProductRepository(IDbConnectionFactory connectionFactory) : IProductRepository
{
    private readonly List<Product> _pendingInserts = [];
    private readonly List<Product> _pendingUpdates = [];
    private readonly List<Product> _pendingDeletes = [];

    public async Task<Product?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        const string sql = """
                           SELECT Id, Name, Description, IsActive, IsDeleted, Price, PriceCurrency, Count, CategoryId, CreatedAt
                           FROM Products
                           WHERE Id = @Id AND IsDeleted = 0
                           """;

        using var connection = connectionFactory.CreateConnection();
        var command = new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken);
        var row = await connection.QuerySingleOrDefaultAsync(command);

        return row is null ? null : MapToProduct(row);
    }

    public async Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        const string sql = """
                           SELECT Id, Name, Description, IsActive, IsDeleted, Price, PriceCurrency, Count, CategoryId, CreatedAt
                           FROM Products
                           WHERE IsDeleted = 0
                           """;

        using var connection = connectionFactory.CreateConnection();
        var command = new CommandDefinition(sql, cancellationToken: cancellationToken);
        var rows = await connection.QueryAsync(command);

        return rows.Select(MapToProduct).ToList();
    }

    public Task AddAsync(Product entity, CancellationToken cancellationToken = default)
    {
        _pendingInserts.Add(entity);
        return Task.CompletedTask;
    }

    public void Update(Product entity) => _pendingUpdates.Add(entity);

    public void Remove(Product entity) => _pendingDeletes.Add(entity);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var affected = 0;
        using var connection = connectionFactory.CreateConnection();

        const string insertSql = """
                                 INSERT INTO Products (Name, Description, IsActive, IsDeleted, Price, PriceCurrency, Count, CategoryId, CreatedAt)
                                 OUTPUT INSERTED.Id
                                 VALUES (@Name, @Description, @IsActive, @IsDeleted, @PriceAmount, @PriceCurrency, @Count, @CategoryId, @CreatedAt)
                                 """;

        foreach (var product in _pendingInserts)
        {
            var command =
                new CommandDefinition(insertSql, ToInsertParams(product), cancellationToken: cancellationToken);
            var generatedId = await connection.ExecuteScalarAsync<long>(command);
            SetPrivateProperty(product, nameof(Product.Id), generatedId);
            affected++;
        }

        const string updateSql = """
                                 UPDATE Products
                                 SET Name = @Name, Description = @Description, IsActive = @IsActive, IsDeleted = @IsDeleted,
                                     Price = @PriceAmount, PriceCurrency = @PriceCurrency, Count = @Count, CategoryId = @CategoryId
                                 WHERE Id = @Id
                                 """;

        foreach (var product in _pendingUpdates)
        {
            var command =
                new CommandDefinition(updateSql, ToUpdateParams(product), cancellationToken: cancellationToken);
            affected += await connection.ExecuteAsync(command);
        }

        const string deleteSql = "DELETE FROM Products WHERE Id = @Id";
        foreach (var product in _pendingDeletes)
        {
            var command = new CommandDefinition(deleteSql, new { product.Id }, cancellationToken: cancellationToken);
            affected += await connection.ExecuteAsync(command);
        }

        _pendingInserts.Clear();
        _pendingUpdates.Clear();
        _pendingDeletes.Clear();

        return affected;
    }

    private static object ToInsertParams(Product product) => new
    {
        product.Name,
        product.Description,
        product.IsActive,
        product.IsDeleted,
        PriceAmount = product.Price.Amount,
        PriceCurrency = (byte)product.Price.Currency,
        product.Count,
        product.CategoryId,
        product.CreatedAt
    };

    private static object ToUpdateParams(Product product) => new
    {
        product.Id,
        product.Name,
        product.Description,
        product.IsActive,
        product.IsDeleted,
        PriceAmount = product.Price.Amount,
        PriceCurrency = (byte)product.Price.Currency,
        product.Count,
        product.CategoryId
    };

    // Product's constructors enforce invariants meant for creating brand-new products, not for
    // rehydrating one from a database row, so materialization bypasses them entirely (same trick
    // EF Core itself uses) and populates the private-set properties directly via reflection.
    private static Product MapToProduct(dynamic row)
    {
        var product = (Product)RuntimeHelpers.GetUninitializedObject(typeof(Product));

        SetPrivateProperty(product, nameof(Product.Id), (long)row.Id);
        SetPrivateProperty(product, nameof(Product.Name), (string)row.Name);
        SetPrivateProperty(product, nameof(Product.Description), (string?)row.Description);
        SetPrivateProperty(product, nameof(Product.IsActive), (bool)row.IsActive);
        SetPrivateProperty(product, nameof(Product.IsDeleted), (bool)row.IsDeleted);
        SetPrivateProperty(product, nameof(Product.Price),
            new Money((decimal)row.Price, (Currency)(byte)row.PriceCurrency));
        SetPrivateProperty(product, nameof(Product.Count), (int)row.Count);
        SetPrivateProperty(product, nameof(Product.CategoryId), (long)row.CategoryId);
        SetPrivateProperty(product, nameof(Product.CreatedAt), (DateTimeOffset)row.CreatedAt);

        return product;
    }

    private static void SetPrivateProperty(object target, string propertyName, object? value)
    {
        var property = target.GetType()
            .GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        property?.SetValue(target, value);
    }
}