using Microsoft.OData.ModelBuilder;
using Shop.Domain.Entities;
using Shop.Domain.Enums;
using Shop.Domain.ValueObjects;

namespace Shop.Api.OData;

public static class ODataEdmModelBuilder
{
    public static Microsoft.OData.Edm.IEdmModel GetEdmModel()
    {
        var builder = new ODataConventionModelBuilder();

        var products = builder.EntitySet<Product>("Products");
        var product = products.EntityType;
        product.HasKey(item => item.Id);
        product.Property(item => item.Name);
        product.Property(item => item.Description);
        product.Property(item => item.IsActive);
        product.ComplexProperty(item => item.Price);
        product.Property(item => item.StockQuantity);
        product.Property(item => item.CategoryId);
        product.Property(item => item.CreatedAt);
        product.HasOptional(item => item.Category);

        var category = builder.EntityType<Category>();
        category.HasKey(item => item.Id);
        category.Property(item => item.Name);
        category.Property(item => item.Description);
        category.Property(item => item.CreatedAt);

        var money = builder.ComplexType<Money>();
        money.Property(value => value.Amount);
        builder.EnumType<Currency>();
        money.EnumProperty(value => value.Currency);

        return builder.GetEdmModel();
    }
}
