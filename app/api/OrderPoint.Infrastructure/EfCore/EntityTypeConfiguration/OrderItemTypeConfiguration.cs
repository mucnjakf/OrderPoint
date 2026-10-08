using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderPoint.Domain.Entities;

namespace OrderPoint.Infrastructure.EfCore.EntityTypeConfiguration;

internal sealed class OrderItemTypeConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("OrderItems");

        builder.HasKey(orderItem => orderItem.Id);

        builder
            .Property(orderItem => orderItem.Id)
            .ValueGeneratedNever()
            .IsRequired();

        builder
            .Property(orderItem => orderItem.Quantity)
            .IsRequired();

        builder
            .Property(orderItem => orderItem.UnitPrice)
            .IsRequired();

        builder
            .Property(orderItem => orderItem.CreatedAtUtc)
            .IsRequired();

        builder
            .Property(orderItem => orderItem.UpdatedAtUtc)
            .IsRequired(false);

        builder
            .HasIndex(orderItem => new { orderItem.OrderId, orderItem.ItemId })
            .IsUnique();

        builder
            .HasOne<Order>()
            .WithMany(order => order.Items)
            .HasForeignKey(orderItem => orderItem.OrderId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        builder
            .HasOne(orderItem => orderItem.Item)
            .WithMany(item => item.OrderItems)
            .HasForeignKey(orderItem => orderItem.ItemId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();
    }
}