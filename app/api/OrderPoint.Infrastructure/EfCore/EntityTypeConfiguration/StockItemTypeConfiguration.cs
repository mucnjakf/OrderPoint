using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderPoint.Domain.Entities;

namespace OrderPoint.Infrastructure.EfCore.EntityTypeConfiguration;

internal sealed class StockItemTypeConfiguration : IEntityTypeConfiguration<StockItem>
{
    public void Configure(EntityTypeBuilder<StockItem> builder)
    {
        builder.ToTable("StockItems");

        builder.HasKey(stockItem => stockItem.Id);

        builder
            .Property(stockItem => stockItem.Id)
            .ValueGeneratedNever()
            .IsRequired();

        builder
            .Property(stockItem => stockItem.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder
            .HasIndex(stockItem => stockItem.Name)
            .IsUnique();

        builder
            .Property(stockItem => stockItem.Group)
            .IsRequired();

        builder
            .Property(stockItem => stockItem.Unit)
            .IsRequired();

        builder
            .Property(stockItem => stockItem.PackName)
            .HasMaxLength(30)
            .IsRequired();

        builder
            .Property(stockItem => stockItem.PackSize)
            .HasPrecision(12, 3)
            .IsRequired();

        builder
            .Property(stockItem => stockItem.PackCost)
            .HasPrecision(10, 2)
            .IsRequired();

        builder
            .Property(stockItem => stockItem.ParLevel)
            .HasPrecision(12, 3)
            .IsRequired();

        builder
            .Property(stockItem => stockItem.Quantity)
            .HasPrecision(12, 3)
            .IsRequired();

        builder
            .Property(stockItem => stockItem.Status)
            .IsRequired();

        builder
            .Property(stockItem => stockItem.CreatedAtUtc)
            .IsRequired();

        builder
            .Property(stockItem => stockItem.UpdatedAtUtc)
            .IsRequired(false);

        builder
            .HasOne(stockItem => stockItem.Supplier)
            .WithMany()
            .HasForeignKey(stockItem => stockItem.SupplierId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);
    }
}