using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderPoint.Domain.Entities;

namespace OrderPoint.Infrastructure.EfCore.EntityTypeConfiguration;

internal sealed class RecipeLineTypeConfiguration : IEntityTypeConfiguration<RecipeLine>
{
    public void Configure(EntityTypeBuilder<RecipeLine> builder)
    {
        builder.ToTable("RecipeLines");

        builder.HasKey(recipeLine => recipeLine.Id);

        builder
            .Property(recipeLine => recipeLine.Id)
            .ValueGeneratedNever()
            .IsRequired();

        builder
            .Property(recipeLine => recipeLine.Quantity)
            .HasPrecision(12, 3)
            .IsRequired();

        builder
            .Property(recipeLine => recipeLine.CreatedAtUtc)
            .IsRequired();

        builder
            .Property(recipeLine => recipeLine.UpdatedAtUtc)
            .IsRequired(false);

        builder
            .HasIndex(recipeLine => new { recipeLine.ItemId, recipeLine.StockItemId })
            .IsUnique();

        builder
            .HasOne(recipeLine => recipeLine.Item)
            .WithMany(item => item.RecipeLines)
            .HasForeignKey(recipeLine => recipeLine.ItemId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        builder
            .HasOne(recipeLine => recipeLine.StockItem)
            .WithMany()
            .HasForeignKey(recipeLine => recipeLine.StockItemId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();
    }
}