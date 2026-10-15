using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderPoint.Domain.Entities;

namespace OrderPoint.Infrastructure.EfCore.EntityTypeConfiguration;

internal sealed class SupplierTypeConfiguration : IEntityTypeConfiguration<Supplier>
{
    public void Configure(EntityTypeBuilder<Supplier> builder)
    {
        builder.ToTable("Suppliers");

        builder.HasKey(supplier => supplier.Id);

        builder
            .Property(supplier => supplier.Id)
            .ValueGeneratedNever()
            .IsRequired();

        builder
            .Property(supplier => supplier.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder
            .HasIndex(supplier => supplier.Name)
            .IsUnique();

        builder
            .Property(supplier => supplier.ContactName)
            .HasMaxLength(100)
            .IsRequired(false);

        builder
            .Property(supplier => supplier.Phone)
            .HasMaxLength(30)
            .IsRequired(false);

        builder
            .Property(supplier => supplier.Email)
            .HasMaxLength(100)
            .IsRequired(false);

        builder
            .Property(supplier => supplier.Note)
            .HasMaxLength(200)
            .IsRequired(false);

        builder
            .Property(supplier => supplier.Status)
            .IsRequired();

        builder
            .Property(supplier => supplier.CreatedAtUtc)
            .IsRequired();

        builder
            .Property(supplier => supplier.UpdatedAtUtc)
            .IsRequired(false);
    }
}