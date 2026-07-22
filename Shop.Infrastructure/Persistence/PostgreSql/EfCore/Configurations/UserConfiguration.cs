using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shop.Domain.Constants;
using Shop.Domain.Entities;

namespace Shop.Infrastructure.Persistence.PostgreSql.EfCore.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.UserName)
            .HasMaxLength(100)
            .IsRequired(false);

        builder.Property(u => u.PasswordHash)
            .IsRequired(false);

        builder.Property(u => u.FirstName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(u => u.LastName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(u => u.PhoneNumber)
            .HasMaxLength(20)
            .IsRequired();

        builder.OwnsOne(u => u.Address, address =>
        {
            address.Property(a => a.RawAddress)
                .HasColumnName("AddressRawAddress")
                .HasMaxLength(AddressConstants.RawAddressMaxLength)
                .IsRequired();

            address.Property(a => a.PostalCode)
                .HasColumnName("AddressPostalCode")
                .HasMaxLength(AddressConstants.PostalCodeMaxLength)
                .IsRequired();
        });
    }
}
