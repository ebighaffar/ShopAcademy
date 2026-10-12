using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shop.Domain.Constants;
using Shop.Domain.Entities;

namespace Shop.Infrastructure.Persistence.PostgreSql.EfCore.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(o => o.Id);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.OwnsOne(o => o.ShippingAddress, address =>
        {
            address.Property(value => value.RawAddress)
                .HasColumnName("ShippingRawAddress")
                .HasMaxLength(AddressConstants.RawAddressMaxLength)
                .IsRequired();
            address.Property(value => value.PostalCode)
                .HasColumnName("ShippingPostalCode")
                .HasMaxLength(AddressConstants.PostalCodeMaxLength)
                .IsRequired();
        });

        builder.HasMany(o => o.Items)
            .WithOne()
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(o => o.Items)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
