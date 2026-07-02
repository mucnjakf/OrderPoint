using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderPoint.Domain.Entities;

namespace OrderPoint.Infrastructure.EfCore.EntityTypeConfiguration;

internal sealed class CategoryTypeConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");

        builder.HasKey(category => category.Id);

        builder
            .Property(category => category.Id)
            .ValueGeneratedNever()
            .IsRequired();

        builder
            .Property(category => category.Name)
            .HasMaxLength(30)
            .IsRequired();

        builder
            .Property(category => category.Description)
            .HasMaxLength(100)
            .IsRequired();

        builder
            .Property(category => category.Status)
            .IsRequired();

        builder
            .Property(category => category.ImageUrl)
            .HasMaxLength(200)
            .IsRequired(false);

        builder
            .Property(category => category.CreatedAtUtc)
            .IsRequired();

        builder
            .Property(category => category.UpdatedAtUtc)
            .IsRequired(false);
    }
}