using MudBlazor;
using OrderPoint.Admin.Health.Enumerations;

namespace OrderPoint.Admin.Shared.Extensions;

// A null status means unknown: not checked yet, or the API that runs the check could not be reached
internal static class HealthStatusExtensions
{
    internal static Color GetColor(this HealthStatus? status)
    {
        return status switch
        {
            HealthStatus.Healthy => Color.Success,
            HealthStatus.Degraded => Color.Warning,
            HealthStatus.Unhealthy => Color.Error,
            null => Color.Default,
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
        };
    }

    internal static string GetIcon(this HealthStatus? status)
    {
        return status switch
        {
            HealthStatus.Healthy => Icons.Material.Filled.CheckCircle,
            HealthStatus.Degraded => Icons.Material.Filled.Warning,
            HealthStatus.Unhealthy => Icons.Material.Filled.Error,
            null => Icons.Material.Filled.Help,
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
        };
    }

    internal static string GetLabel(this HealthStatus? status)
    {
        return status?.ToString() ?? "Unknown";
    }

    internal static string GetSummary(this HealthStatus? status)
    {
        return status switch
        {
            HealthStatus.Healthy => "All systems operational",
            HealthStatus.Degraded => "Some systems are degraded",
            HealthStatus.Unhealthy => "Some systems are down",
            null => "Not checked yet",
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
        };
    }
}