using Microsoft.JSInterop;

namespace OrderPoint.Admin.Shared.Services;

internal sealed class TimeZoneService(IJSRuntime jsRuntime)
{
    internal TimeZoneInfo TimeZone { get; private set; } = TimeZoneInfo.Utc;

    internal async Task LoadAsync()
    {
        await using IJSObjectReference dateTimeFormat =
            await jsRuntime.InvokeConstructorAsync("Intl.DateTimeFormat");

        DateTimeFormatOptions options = await dateTimeFormat.InvokeAsync<DateTimeFormatOptions>("resolvedOptions");

        if (TimeZoneInfo.TryFindSystemTimeZoneById(options.TimeZone, out TimeZoneInfo? timeZone))
        {
            TimeZone = timeZone;
        }
    }

    private sealed record DateTimeFormatOptions(string TimeZone);
}