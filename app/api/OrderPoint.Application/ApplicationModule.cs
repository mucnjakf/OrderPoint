using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderPoint.Application.Mediator;
using OrderPoint.Application.Notifications;
using OrderPoint.Application.Queries.Dashboard;
using OrderPoint.Application.Telemetry;

namespace OrderPoint.Application;

public static class ApplicationModule
{
    private const string BusinessDayStartHourKey = "BusinessDay:StartHour";

    private const int HoursPerDay = 24;
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
            configure.AddOpenBehavior(typeof(MenuNotificationBehavior<,>));
            configure.AddOpenBehavior(typeof(TablesNotificationBehavior<,>));
            configure.AddOpenBehavior(typeof(CacheInvalidationBehavior<,>));
        });

        services.AddSingleton<ApplicationTelemetry>();
        services.AddScoped<ChangedOrders>();
        services.AddScoped<ChangedMenu>();
        services.AddSingleton(new BusinessDay(GetBusinessDayStartTime(configuration)));

        return services;
    }

    // Midnight when the setting is missing; a wrong value stops the API instead of silently moving every "today"
    private static TimeSpan GetBusinessDayStartTime(IConfiguration configuration)
    {
        string? value = configuration[BusinessDayStartHourKey];

        if (value is null)
        {
            return TimeSpan.Zero;
        }

        if (!int.TryParse(value, out int startHour) || startHour is < 0 or >= HoursPerDay)
        {
            throw new InvalidOperationException($"{BusinessDayStartHourKey} must be an hour from 0 to 23");
        }

        return TimeSpan.FromHours(startHour);
    }
}