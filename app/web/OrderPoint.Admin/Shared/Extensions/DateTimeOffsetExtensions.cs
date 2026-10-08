namespace OrderPoint.Admin.Shared.Extensions;

internal static class DateTimeOffsetExtensions
{
    internal static string ToRelativeTime(this DateTimeOffset dateTime)
    {
        TimeSpan elapsed = DateTimeOffset.UtcNow - dateTime;

        return elapsed switch
        {
            { TotalMinutes: < 1 } => "just now",
            { TotalHours: < 1 } => $"{(int)elapsed.TotalMinutes} min ago",
            { TotalDays: < 1 } => $"{(int)elapsed.TotalHours} h ago",
            { TotalDays: < 2 } => "1 day ago",
            _ => $"{(int)elapsed.TotalDays} days ago"
        };
    }
}