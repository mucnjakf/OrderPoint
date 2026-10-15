using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderPoint.Domain.Entities;

namespace OrderPoint.Infrastructure.EfCore.EntityTypeConfiguration;

internal sealed class DeliveryLineTypeConfiguration : IEntityTypeConfiguration<DeliveryLine>
{
    public void Configure(EntityTypeBuilder<DeliveryLine> builder)
    {
        builder.ToTable("DeliveryLines");

        builder.HasKey(line => line.Id);

        builder
            .Property(line => line.Id)
            .ValueGeneratedNever()
            .IsRequired();

        builder
            .Property(line => line.Packs)
            .HasPrecision(12, 3)
            .IsRequired();

        builder
            .Property(line => line.PackCost)
            .HasPrecision(10, 2)
            .IsRequired();

        builder
            .Property(line => line.CreatedAtUtc)
            .IsRequired();

        builder
            .Property(line => line.UpdatedAtUtc)
            .IsRequired(false);

        builder
            .HasIndex(line => new { line.DeliveryId, line.StockItemId })
            .IsUnique();

        builder
            .HasOne<Delivery>()
            .WithMany(delivery => delivery.Lines)
            .HasForeignKey(line => line.DeliveryId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        builder
            .HasOne(line => line.StockItem)
            .WithMany()
            .HasForeignKey(line => line.StockItemId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();
    }
}