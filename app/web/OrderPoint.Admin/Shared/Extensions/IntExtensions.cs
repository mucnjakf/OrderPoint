namespace OrderPoint.Admin.Shared.Extensions;

internal static class IntExtensions
{
    internal static string ToCountText(this int count, string singular, string plural)
    {
        return count == 1 ? $"{count} {singular}" : $"{count} {plural}";
    }
}