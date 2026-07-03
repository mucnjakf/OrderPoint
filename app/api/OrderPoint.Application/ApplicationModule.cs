using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace OrderPoint.Application;

public static class ApplicationModule
{
    public static IServiceCollection AddApplicationModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddMediatR(configure =>
        {
            configure.LicenseKey = configuration["MediatR:LicenseKey"];
            configure.RegisterServicesFromAssembly(typeof(ApplicationModule).Assembly);
        });

        return services;
    }
}