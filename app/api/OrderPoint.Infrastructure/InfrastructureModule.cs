using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderPoint.Application.Identity;
using OrderPoint.Application.Payments;
using OrderPoint.Application.Repositories;
using OrderPoint.Application.Storage;
using OrderPoint.Infrastructure.EfCore;
using OrderPoint.Infrastructure.EfCore.Repositories;
using OrderPoint.Infrastructure.Identity;
using OrderPoint.Infrastructure.Payments;
using OrderPoint.Infrastructure.Storage;

namespace OrderPoint.Infrastructure;

public static class InfrastructureModule
{
    private const int MinimumPasswordLength = 8;

    private static readonly TimeSpan LocalCacheExpiration = TimeSpan.FromMinutes(5);

    private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(30);

    public static IServiceCollection AddInfrastructureModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("order-point-db"));
        });

        services.AddScoped<IUnitOfWork>(serviceProvider => serviceProvider.GetRequiredService<ApplicationDbContext>());

        services.AddScoped<ICategoryRepository, CategoryEfCoreRepository>();
        services.AddScoped<IItemRepository, ItemEfCoreRepository>();
        services.AddScoped<IBartenderRepository, BartenderEfCoreRepository>();
        services.AddScoped<IOrderRepository, OrderEfCoreRepository>();
        services.AddScoped<IAreaRepository, AreaEfCoreRepository>();
        services.AddScoped<ITableRepository, TableEfCoreRepository>();
        services.AddScoped<IFixtureRepository, FixtureEfCoreRepository>();
        services.AddScoped<ITableRequestRepository, TableRequestEfCoreRepository>();
        services.AddScoped<IStockItemRepository, StockItemEfCoreRepository>();
        services.AddScoped<IStockMovementRepository, StockMovementEfCoreRepository>();
        services.AddScoped<IRecipeLineRepository, RecipeLineEfCoreRepository>();
        services.AddScoped<IDashboardRepository, DashboardEfCoreRepository>();

        services.AddScoped<IImageStorage, BlobImageStorage>();
        services.AddScoped<IPaymentProvider, SimulatedPaymentProvider>();

        // In memory and, through the IDistributedCache the API registers, in Redis; writes remove entries by tag
        services.AddHybridCache(options => options.DefaultEntryOptions = new HybridCacheEntryOptions
        {
            LocalCacheExpiration = LocalCacheExpiration,
            Expiration = CacheExpiration
        });

        // Lockout defaults: 5 failed attempts lock the account for 5 minutes
        services.AddIdentityCore<ApplicationUser>(options =>
        {
            options.Password.RequiredLength = MinimumPasswordLength;
            options.Password.RequireNonAlphanumeric = false;
            options.User.RequireUniqueEmail = true;
        });

        // Identity's user-only store works on a plain DbContext and only touches the users table for what we use.
        // Without auto-save, user changes are persisted together with everything else by IUnitOfWork.
        services.AddScoped<IUserStore<ApplicationUser>>(serviceProvider =>
            new ApplicationUserStore(serviceProvider.GetRequiredService<ApplicationDbContext>())
            {
                AutoSaveChanges = false
            });

        services
            .AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .Validate(
                options => options.SigningKey.Length >= JwtOptions.MinimumSigningKeyLength,
                $"{JwtOptions.SectionName}:{nameof(JwtOptions.SigningKey)} must be at least " +
                $"{JwtOptions.MinimumSigningKeyLength} characters")
            .ValidateOnStart();

        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<ITokenService, TokenService>();

        return services;
    }
}