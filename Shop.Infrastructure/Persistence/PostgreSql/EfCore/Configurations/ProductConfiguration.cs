using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shop.Domain.Constants;
using Shop.Domain.Entities;

namespace Shop.Infrastructure.Persistence.PostgreSql.EfCore.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name)
            .HasMaxLength(ProductConstants.NameMaxLength)
            .IsRequired();

        builder.Property(p => p.Description)
            .HasMaxLength(ProductConstants.DescriptionMaxLength)
            .IsRequired(false)
            .IsUnicode();

        builder.Property(p => p.IsActive)
            .IsRequired();

        // Maps to PostgreSQL's xmin system column: optimistic concurrency without an extra column.
        builder.Property(p => p.Version)
            .IsRowVersion();

        builder.Property(p => p.IsDeleted)
            .IsRequired();

        builder.HasQueryFilter(p => !p.IsDeleted);

        builder.OwnsOne(p => p.Price, price =>
        {
            price.Property(m => m.Amount)
                .HasColumnName("Price")
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            price.Property(m => m.Currency)
                .HasColumnName("PriceCurrency")
                .IsRequired();
        });

        builder.HasOne(p => p.Category)
            .WithMany()
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}