using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderPoint.Domain.Entities;

namespace OrderPoint.Infrastructure.EfCore.EntityTypeConfiguration;

internal sealed class BartenderTypeConfiguration : IEntityTypeConfiguration<Bartender>
{
    public void Configure(EntityTypeBuilder<Bartender> builder)
    {
        builder.ToTable("Bartenders");

        builder.HasKey(bartender => bartender.Id);

        builder
            .Property(bartender => bartender.Id)
            .ValueGeneratedNever()
            .IsRequired();

        builder
            .Property(bartender => bartender.FirstName)
            .HasMaxLength(30)
            .IsRequired();

        builder
            .Property(bartender => bartender.LastName)
            .HasMaxLength(30)
            .IsRequired();

        builder
            .Property(bartender => bartender.Email)
            .HasMaxLength(100)
            .IsRequired();

        builder
            .HasIndex(bartender => bartender.Email)
            .IsUnique();

        builder
            .Property(bartender => bartender.PhoneNumber)
            .HasMaxLength(20)
            .IsRequired(false);

        builder
            .Property(bartender => bartender.Status)
            .IsRequired();

        builder
            .Property(bartender => bartender.Notes)
            .HasMaxLength(500)
            .IsRequired(false);

        builder
            .Property(bartender => bartender.ImageUrl)
            .HasMaxLength(200)
            .IsRequired(false);

        builder
            .Property(bartender => bartender.CreatedAtUtc)
            .IsRequired();

        builder
            .Property(bartender => bartender.UpdatedAtUtc)
            .IsRequired(false);
    }
}