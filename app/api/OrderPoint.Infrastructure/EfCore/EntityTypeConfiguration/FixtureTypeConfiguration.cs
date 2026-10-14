using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderPoint.Domain.Entities;

namespace OrderPoint.Infrastructure.EfCore.EntityTypeConfiguration;

internal sealed class FixtureTypeConfiguration : IEntityTypeConfiguration<Fixture>
{
    public void Configure(EntityTypeBuilder<Fixture> builder)
    {
        builder.ToTable("Fixtures");

        builder.HasKey(fixture => fixture.Id);

        builder
            .Property(fixture => fixture.Id)
            .ValueGeneratedNever()
            .IsRequired();

        builder
            .Property(fixture => fixture.Type)
            .IsRequired();

        builder
            .Property(fixture => fixture.Label)
            .HasMaxLength(30)
            .IsRequired(false);

        builder
            .Property(fixture => fixture.X)
            .IsRequired();

        builder
            .Property(fixture => fixture.Y)
            .IsRequired();

        builder
            .Property(fixture => fixture.Width)
            .IsRequired();

        builder
            .Property(fixture => fixture.Height)
            .IsRequired();

        builder
            .Property(fixture => fixture.CreatedAtUtc)
            .IsRequired();

        builder
            .Property(fixture => fixture.UpdatedAtUtc)
            .IsRequired(false);

        builder
            .HasOne<Area>()
            .WithMany(area => area.Fixtures)
            .HasForeignKey(fixture => fixture.AreaId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();
    }
}