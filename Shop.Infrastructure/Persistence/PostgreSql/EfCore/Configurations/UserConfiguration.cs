using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shop.Domain.Constants;
using Shop.Domain.Entities;

namespace Shop.Infrastructure.Persistence.PostgreSql.EfCore.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(user => user.Id);

        builder.Property(user => user.UserName)
            .HasMaxLength(100)
            .IsRequired();
        builder.HasIndex(user => user.UserName).IsUnique();

        builder.Property(user => user.Email)
            .HasMaxLength(200)
            .IsRequired();
        builder.HasIndex(user => user.Email).IsUnique();

        builder.Property(user => user.PasswordHash)
            .HasMaxLength(500)
            .IsRequired();
        builder.Property(user => user.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(user => user.LastName).HasMaxLength(100).IsRequired();
        builder.Property(user => user.PhoneNumber).HasMaxLength(20).IsRequired();

        builder.OwnsOne(user => user.Address, address =>
        {
            address.Property(value => value.RawAddress)
                .HasColumnName("AddressRawAddress")
                .HasMaxLength(AddressConstants.RawAddressMaxLength)
                .IsRequired();
            address.Property(value => value.PostalCode)
                .HasColumnName("AddressPostalCode")
                .HasMaxLength(AddressConstants.PostalCodeMaxLength)
                .IsRequired();
        });

        builder.Navigation(user => user.Roles)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(user => user.Permissions)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(user => user.RefreshTokens)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
