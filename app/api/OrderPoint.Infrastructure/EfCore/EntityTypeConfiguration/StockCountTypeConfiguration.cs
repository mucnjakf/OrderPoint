using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderPoint.Domain.Entities;

namespace OrderPoint.Infrastructure.EfCore.EntityTypeConfiguration;

internal sealed class StockCountTypeConfiguration : IEntityTypeConfiguration<StockCount>
{
    public void Configure(EntityTypeBuilder<StockCount> builder)
    {
        builder.ToTable("StockCounts");

        builder.HasKey(stockCount => stockCount.Id);

        builder
            .Property(stockCount => stockCount.Id)
            .ValueGeneratedNever()
            .IsRequired();

        builder
            .PrimitiveCollection(stockCount => stockCount.Groups)
            .IsRequired();

        builder
            .Property(stockCount => stockCount.Status)
            .IsRequired();

        builder
            .Property(stockCount => stockCount.Note)
            .HasMaxLength(200)
            .IsRequired(false);

        builder
            .Property(stockCount => stockCount.ClosedAtUtc)
            .IsRequired(false);

        builder
            .Property(stockCount => stockCount.CreatedAtUtc)
            .IsRequired();

        builder
            .Property(stockCount => stockCount.UpdatedAtUtc)
            .IsRequired(false);
    }
}