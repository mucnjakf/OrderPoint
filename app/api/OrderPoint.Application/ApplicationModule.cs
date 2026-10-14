using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderPoint.Application.Mediator;
using OrderPoint.Application.Notifications;
using OrderPoint.Application.Telemetry;

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
            configure.AddOpenBehavior(typeof(TelemetryBehavior<,>));
            configure.AddOpenBehavior(typeof(OrderNotificationBehavior<,>));
            configure.AddOpenBehavior(typeof(CacheInvalidationBehavior<,>));
        });

        services.AddSingleton<ApplicationTelemetry>();
        services.AddScoped<ChangedOrders>();

        return services;
    }
}