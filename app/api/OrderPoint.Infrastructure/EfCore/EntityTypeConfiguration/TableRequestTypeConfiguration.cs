using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderPoint.Domain.Entities;

namespace OrderPoint.Infrastructure.EfCore.EntityTypeConfiguration;

internal sealed class TableRequestTypeConfiguration : IEntityTypeConfiguration<TableRequest>
{
    public void Configure(EntityTypeBuilder<TableRequest> builder)
    {
        builder.ToTable("TableRequests");

        builder.HasKey(request => request.Id);

        builder
            .Property(request => request.Id)
            .ValueGeneratedNever()
            .IsRequired();

        builder
            .Property(request => request.Type)
            .IsRequired();

        builder
            .Property(request => request.Status)
            .IsRequired();

        builder
            .Property(request => request.DoneAtUtc)
            .IsRequired(false);

        builder
            .Property(request => request.CreatedAtUtc)
            .IsRequired();

        builder
            .Property(request => request.UpdatedAtUtc)
            .IsRequired(false);

        builder
            .HasOne(request => request.Table)
            .WithMany()
            .HasForeignKey(request => request.TableId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();
    }
}