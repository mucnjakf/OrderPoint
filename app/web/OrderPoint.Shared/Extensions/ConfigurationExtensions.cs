using Microsoft.Extensions.Configuration;

namespace OrderPoint.Shared.Extensions;

internal static class ConfigurationExtensions
{
    // Set by the AppHost, like for the API; midnight when missing
    private const string BusinessDayStartHourKey = "BusinessDay:StartHour";

    // When the bar's day starts, local time, so a night past midnight is one day
    internal static TimeSpan GetBusinessDayStart(this IConfiguration configuration)
    {
        return TimeSpan.FromHours(configuration.GetValue<int>(BusinessDayStartHourKey));
    }
}