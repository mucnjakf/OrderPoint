using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderPoint.Domain.Entities;

namespace OrderPoint.Infrastructure.EfCore.EntityTypeConfiguration;

internal sealed class AreaTypeConfiguration : IEntityTypeConfiguration<Area>
{
    public void Configure(EntityTypeBuilder<Area> builder)
    {
        builder.ToTable("Areas");

        builder.HasKey(area => area.Id);

        builder
            .Property(area => area.Id)
            .ValueGeneratedNever()
            .IsRequired();

        builder
            .Property(area => area.Name)
            .HasMaxLength(30)
            .IsRequired();

        builder
            .HasIndex(area => area.Name)
            .IsUnique();

        builder
            .Property(area => area.Columns)
            .IsRequired();

        builder
            .Property(area => area.Rows)
            .IsRequired();

        builder
            .Property(area => area.DisplayOrder)
            .IsRequired();

        builder
            .Property(area => area.CreatedAtUtc)
            .IsRequired();

        builder
            .Property(area => area.UpdatedAtUtc)
            .IsRequired(false);
    }
}