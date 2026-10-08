using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderPoint.Infrastructure.Identity;

namespace OrderPoint.Infrastructure.EfCore.EntityTypeConfiguration;

internal sealed class ApplicationUserTypeConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(user => user.Id);

        builder
            .Property(user => user.Id)
            .ValueGeneratedNever()
            .IsRequired();

        builder
            .Property(user => user.UserName)
            .HasMaxLength(100)
            .IsRequired();

        builder
            .Property(user => user.NormalizedUserName)
            .HasMaxLength(100)
            .IsRequired();

        builder
            .HasIndex(user => user.NormalizedUserName)
            .IsUnique();

        builder
            .Property(user => user.Email)
            .HasMaxLength(100)
            .IsRequired();

        builder
            .Property(user => user.NormalizedEmail)
            .HasMaxLength(100)
            .IsRequired();

        builder
            .HasIndex(user => user.NormalizedEmail)
            .IsUnique();

        builder
            .Property(user => user.PasswordHash)
            .IsRequired();

        builder
            .Property(user => user.SecurityStamp)
            .IsRequired();

        builder
            .Property(user => user.ConcurrencyStamp)
            .IsConcurrencyToken()
            .IsRequired();

        builder
            .Property(user => user.LockoutEnabled)
            .IsRequired();

        builder
            .Property(user => user.LockoutEnd)
            .IsRequired(false);

        builder
            .Property(user => user.AccessFailedCount)
            .IsRequired();

        builder
            .Property(user => user.Role)
            .IsRequired();

        builder
            .Property(user => user.MustChangePassword)
            .IsRequired();

        // Not used: phone numbers live on Bartender, and there is no email confirmation or two-factor login
        builder.Ignore(user => user.PhoneNumber);
        builder.Ignore(user => user.PhoneNumberConfirmed);
        builder.Ignore(user => user.EmailConfirmed);
        builder.Ignore(user => user.TwoFactorEnabled);
    }
}