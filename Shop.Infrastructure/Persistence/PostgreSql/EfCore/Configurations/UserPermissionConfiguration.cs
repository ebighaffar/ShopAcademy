using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shop.Domain.Entities;

namespace Shop.Infrastructure.Persistence.PostgreSql.EfCore.Configurations;

public class UserPermissionConfiguration : IEntityTypeConfiguration<UserPermission>
{
    public void Configure(EntityTypeBuilder<UserPermission> builder)
    {
        builder.HasKey(item => new { item.UserId, item.PermissionId });

        builder.HasOne(item => item.User)
            .WithMany(user => user.Permissions)
            .HasForeignKey(item => item.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(item => item.Permission)
            .WithMany()
            .HasForeignKey(item => item.PermissionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(item => item.PermissionId);
    }
}
