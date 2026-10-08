using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderPoint.Domain.Entities;

namespace OrderPoint.Infrastructure.EfCore.EntityTypeConfiguration;

internal sealed class OrderTypeConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");

        builder.HasKey(order => order.Id);

        builder
            .Property(order => order.Id)
            .ValueGeneratedNever()
            .IsRequired();

        builder
            .Property(order => order.Number)
            .UseIdentityAlwaysColumn()
            .IsRequired();

        builder
            .HasIndex(order => order.Number)
            .IsUnique();

        builder
            .Property(order => order.TableCode)
            .HasMaxLength(10)
            .IsRequired();

        builder
            .Property(order => order.Note)
            .HasMaxLength(200)
            .IsRequired(false);

        builder
            .Property(order => order.Status)
            .IsRequired();

        builder
            .Property(order => order.AcceptedAtUtc)
            .IsRequired(false);

        builder
            .Property(order => order.DeclinedAtUtc)
            .IsRequired(false);

        builder
            .Property(order => order.ActivatedAtUtc)
            .IsRequired(false);

        builder
            .Property(order => order.CompletedAtUtc)
            .IsRequired(false);

        builder
            .Property(order => order.CreatedAtUtc)
            .IsRequired();

        builder
            .Property(order => order.UpdatedAtUtc)
            .IsRequired(false);

        builder
            .HasOne(order => order.Bartender)
            .WithMany(bartender => bartender.Orders)
            .HasForeignKey(order => order.BartenderId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);
    }
}