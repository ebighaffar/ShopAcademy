using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shop.Domain.Entities;
using Shop.Domain.Enums;

namespace Shop.Infrastructure.Persistence.PostgreSql.EfCore.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.HasKey(p => p.Id);

        builder.HasOne<Order>()
            .WithMany()
            .HasForeignKey(p => p.OrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.OwnsOne(p => p.Amount, amount =>
        {
            amount.Property(m => m.Amount)
                .HasColumnName("Amount")
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            amount.Property(m => m.Currency)
                .HasColumnName("AmountCurrency")
                .IsRequired();
        });

        builder.Property(p => p.ProviderReference)
            .HasMaxLength(200);

        // An order can have many failed attempts but at most one successful payment.
        builder.HasIndex(p => p.OrderId)
            .IsUnique()
            .HasFilter($"\"Status\" = {(byte)PaymentStatus.Succeeded}");
    }
}
