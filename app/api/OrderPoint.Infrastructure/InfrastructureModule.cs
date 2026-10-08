using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderPoint.Application.Repositories;
using OrderPoint.Application.Storage;
using OrderPoint.Infrastructure.EfCore;
using OrderPoint.Infrastructure.EfCore.Repositories;
using OrderPoint.Infrastructure.Storage;

namespace OrderPoint.Infrastructure;

public static class InfrastructureModule
{
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
        services.AddScoped<IDashboardRepository, DashboardEfCoreRepository>();

        services.AddScoped<IImageStorage, BlobImageStorage>();

        return services;
    }
}