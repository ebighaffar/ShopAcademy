using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shop.Domain.Entities;

namespace Shop.Infrastructure.Persistence.PostgreSql.EfCore.Configurations;

public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.HasKey(item => new { item.RoleId, item.PermissionId });
        builder.HasOne(item => item.Role)
            .WithMany(role => role.Permissions)
            .HasForeignKey(item => item.RoleId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(item => item.Permission)
            .WithMany()
            .HasForeignKey(item => item.PermissionId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(item => item.PermissionId);
    }
}
