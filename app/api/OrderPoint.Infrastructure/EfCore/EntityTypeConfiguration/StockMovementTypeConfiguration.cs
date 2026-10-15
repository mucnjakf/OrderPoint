using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderPoint.Domain.Entities;

namespace OrderPoint.Infrastructure.EfCore.EntityTypeConfiguration;

internal sealed class StockMovementTypeConfiguration : IEntityTypeConfiguration<StockMovement>
{
    public void Configure(EntityTypeBuilder<StockMovement> builder)
    {
        builder.ToTable("StockMovements");

        builder.HasKey(movement => movement.Id);

        builder
            .Property(movement => movement.Id)
            .ValueGeneratedNever()
            .IsRequired();

        builder
            .Property(movement => movement.Type)
            .IsRequired();

        builder
            .Property(movement => movement.QuantityChange)
            .HasPrecision(12, 3)
            .IsRequired();

        builder
            .Property(movement => movement.QuantityAfter)
            .HasPrecision(12, 3)
            .IsRequired();

        builder
            .Property(movement => movement.UnitCost)
            .HasPrecision(12, 6)
            .IsRequired();

        builder
            .Property(movement => movement.Note)
            .HasMaxLength(200)
            .IsRequired(false);

        builder
            .Property(movement => movement.CreatedAtUtc)
            .IsRequired();

        builder
            .Property(movement => movement.UpdatedAtUtc)
            .IsRequired(false);

        builder
            .HasOne(movement => movement.StockItem)
            .WithMany()
            .HasForeignKey(movement => movement.StockItemId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();
    }
}