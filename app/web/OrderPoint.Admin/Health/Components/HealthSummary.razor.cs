using Microsoft.AspNetCore.Components;
using OrderPoint.Admin.Health.Enumerations;
using OrderPoint.Admin.Shared.Extensions;
using OrderPoint.Admin.Shared.Services;

namespace OrderPoint.Admin.Health.Components;

public sealed partial class HealthSummary
{
    [Inject]
    private TimeZoneService TimeZoneService { get; set; } = null!;

    [Parameter]
    [EditorRequired]
    public HealthStatus? Status { get; set; }

    [Parameter]
    [EditorRequired]
    public DateTimeOffset? CheckedAtUtc { get; set; }

    [Parameter]
    [EditorRequired]
    public bool IsLoading { get; set; }

    private string CheckedAtText => CheckedAtUtc is null
        ? "Waiting for the first check"
        : $"Checked at {CheckedAtUtc.Value.ToTimeZone(TimeZoneService.TimeZone):T}";
}