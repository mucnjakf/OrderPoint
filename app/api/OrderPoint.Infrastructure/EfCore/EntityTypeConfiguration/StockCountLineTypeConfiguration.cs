using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderPoint.Domain.Entities;

namespace OrderPoint.Infrastructure.EfCore.EntityTypeConfiguration;

internal sealed class StockCountLineTypeConfiguration : IEntityTypeConfiguration<StockCountLine>
{
    public void Configure(EntityTypeBuilder<StockCountLine> builder)
    {
        builder.ToTable("StockCountLines");

        builder.HasKey(line => line.Id);

        builder
            .Property(line => line.Id)
            .ValueGeneratedNever()
            .IsRequired();

        builder
            .Property(line => line.CountedQuantity)
            .HasPrecision(12, 3)
            .IsRequired(false);

        builder
            .Property(line => line.ExpectedQuantity)
            .HasPrecision(12, 3)
            .IsRequired(false);

        builder
            .Property(line => line.UnitCost)
            .HasPrecision(12, 6)
            .IsRequired(false);

        builder
            .Property(line => line.CreatedAtUtc)
            .IsRequired();

        builder
            .Property(line => line.UpdatedAtUtc)
            .IsRequired(false);

        builder
            .HasIndex(line => new { line.StockCountId, line.StockItemId })
            .IsUnique();

        // The lines belong to their count: discarding an open count deletes them
        builder
            .HasOne<StockCount>()
            .WithMany(stockCount => stockCount.Lines)
            .HasForeignKey(line => line.StockCountId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        builder
            .HasOne(line => line.StockItem)
            .WithMany()
            .HasForeignKey(line => line.StockItemId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();
    }
}