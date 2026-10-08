using Microsoft.EntityFrameworkCore;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Entities;
using OrderPoint.Infrastructure.Identity;

namespace OrderPoint.Infrastructure.EfCore;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options), IUnitOfWork
{
    internal DbSet<Category> Categories { get; init; } = null!;

    internal DbSet<Item> Items { get; init; } = null!;

    internal DbSet<Bartender> Bartenders { get; init; } = null!;

    internal DbSet<Order> Orders { get; init; } = null!;

    internal DbSet<ApplicationUser> Users { get; init; } = null!;

    internal DbSet<RefreshToken> RefreshTokens { get; init; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(InfrastructureModule).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}