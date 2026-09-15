using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shop.Domain.Entities;

namespace Shop.Infrastructure.Persistence.PostgreSql.EfCore.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.HasKey(role => role.Id);
        builder.Property(role => role.Name).HasMaxLength(100).IsRequired();
        builder.HasIndex(role => role.Name).IsUnique();
        builder.Navigation(role => role.Permissions)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
