using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderPoint.Infrastructure.Identity;

namespace OrderPoint.Infrastructure.EfCore.EntityTypeConfiguration;

internal sealed class RefreshTokenTypeConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");

        builder.HasKey(refreshToken => refreshToken.Id);

        builder
            .Property(refreshToken => refreshToken.Id)
            .ValueGeneratedNever()
            .IsRequired();

        builder
            .Property(refreshToken => refreshToken.TokenHash)
            .HasMaxLength(64)
            .IsRequired();

        builder
            .HasIndex(refreshToken => refreshToken.TokenHash)
            .IsUnique();

        builder
            .Property(refreshToken => refreshToken.ExpiresAtUtc)
            .IsRequired();

        builder
            .Property(refreshToken => refreshToken.CreatedAtUtc)
            .IsRequired();

        builder
            .HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(refreshToken => refreshToken.UserId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();
    }
}