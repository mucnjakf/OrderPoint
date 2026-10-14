using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderPoint.Domain.Entities;

namespace OrderPoint.Infrastructure.EfCore.EntityTypeConfiguration;

internal sealed class TableTypeConfiguration : IEntityTypeConfiguration<Table>
{
    public void Configure(EntityTypeBuilder<Table> builder)
    {
        builder.ToTable("Tables");

        builder.HasKey(table => table.Id);

        builder
            .Property(table => table.Id)
            .ValueGeneratedNever()
            .IsRequired();

        builder
            .Property(table => table.Code)
            .HasMaxLength(10)
            .IsRequired();

        builder
            .HasIndex(table => table.Code)
            .IsUnique();

        builder
            .Property(table => table.Seats)
            .IsRequired();

        builder
            .Property(table => table.Status)
            .IsRequired();

        builder
            .Property(table => table.Shape)
            .IsRequired();

        builder
            .Property(table => table.X)
            .IsRequired();

        builder
            .Property(table => table.Y)
            .IsRequired();

        builder
            .Property(table => table.Width)
            .IsRequired();

        builder
            .Property(table => table.Height)
            .IsRequired();

        builder
            .Property(table => table.CreatedAtUtc)
            .IsRequired();

        builder
            .Property(table => table.UpdatedAtUtc)
            .IsRequired(false);

        builder
            .HasOne(table => table.Area)
            .WithMany(area => area.Tables)
            .HasForeignKey(table => table.AreaId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();
    }
}