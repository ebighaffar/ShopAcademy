using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shop.Domain.Constants;
using Shop.Domain.Entities;

namespace Shop.Infrastructure.Persistence.PostgreSql.EfCore.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Name)
            .HasMaxLength(CategoryConstants.NameMaxLength)
            .IsRequired();
        builder.HasIndex(c => c.Name).IsUnique();

        builder.Property(c => c.Description)
            .HasMaxLength(CategoryConstants.DescriptionMaxLength)
            .IsRequired(false)
            .IsUnicode();
    }
}
