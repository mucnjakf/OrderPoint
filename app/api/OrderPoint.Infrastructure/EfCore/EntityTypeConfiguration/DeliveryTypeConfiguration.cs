using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderPoint.Domain.Entities;

namespace OrderPoint.Infrastructure.EfCore.EntityTypeConfiguration;

internal sealed class DeliveryTypeConfiguration : IEntityTypeConfiguration<Delivery>
{
    public void Configure(EntityTypeBuilder<Delivery> builder)
    {
        builder.ToTable("Deliveries");

        builder.HasKey(delivery => delivery.Id);

        builder
            .Property(delivery => delivery.Id)
            .ValueGeneratedNever()
            .IsRequired();

        builder
            .Property(delivery => delivery.DeliveredOn)
            .IsRequired();

        builder
            .Property(delivery => delivery.Reference)
            .HasMaxLength(50)
            .IsRequired(false);

        builder
            .Property(delivery => delivery.Note)
            .HasMaxLength(200)
            .IsRequired(false);

        builder
            .Property(delivery => delivery.CreatedAtUtc)
            .IsRequired();

        builder
            .Property(delivery => delivery.UpdatedAtUtc)
            .IsRequired(false);

        builder
            .HasOne(delivery => delivery.Supplier)
            .WithMany()
            .HasForeignKey(delivery => delivery.SupplierId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();
    }
}