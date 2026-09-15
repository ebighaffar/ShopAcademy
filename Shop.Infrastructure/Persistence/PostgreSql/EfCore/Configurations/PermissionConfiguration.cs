using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shop.Domain.Entities;

namespace Shop.Infrastructure.Persistence.PostgreSql.EfCore.Configurations;

public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.HasKey(permission => permission.Id);
        builder.Property(permission => permission.Name).HasMaxLength(150).IsRequired();
        builder.Property(permission => permission.Description).HasMaxLength(500);
        builder.HasIndex(permission => permission.Name).IsUnique();
    }
}
