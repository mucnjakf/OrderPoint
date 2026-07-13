using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderPoint.Domain.Entities;

namespace OrderPoint.Infrastructure.EfCore.EntityTypeConfiguration;

internal sealed class ItemTypeConfiguration : IEntityTypeConfiguration<Item>
{
    public void Configure(EntityTypeBuilder<Item> builder)
    {
        builder.ToTable("Items");

        builder.HasKey(item => item.Id);

        builder
            .Property(item => item.Id)
            .ValueGeneratedNever()
            .IsRequired();

        builder
            .Property(item => item.Name)
            .HasMaxLength(30)
            .IsRequired();

        builder
            .Property(item => item.Description)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(item => item.Portion)
            .IsRequired();

        builder.Property(item => item.Price)
            .IsRequired();

        builder
            .Property(item => item.ImageUrl)
            .HasMaxLength(200)
            .IsRequired(false);

        builder
            .Property(item => item.CreatedAtUtc)
            .IsRequired();

        builder
            .Property(item => item.UpdatedAtUtc)
            .IsRequired(false);

        builder
            .HasOne(item => item.Category)
            .WithMany(category => category.Items)
            .HasForeignKey(item => item.CategoryId)
            .IsRequired();
    }
}