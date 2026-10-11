using Microsoft.AspNetCore.Components;
using OrderPoint.Admin.Health.Dtos;
using OrderPoint.Admin.Health.Enumerations;

namespace OrderPoint.Admin.Health.Components;

public sealed partial class HealthCheckCard
{
    [Parameter]
    [EditorRequired]
    public string Name { get; set; }

    [Parameter]
    [EditorRequired]
    public string Subtitle { get; set; }

    [Parameter]
    [EditorRequired]
    public string Icon { get; set; }

    // Null while the API that runs the check cannot be reached
    [Parameter]
    [EditorRequired]
    public HealthCheckDto? Check { get; set; }

    [Parameter]
    [EditorRequired]
    public bool IsLoading { get; set; }

    private HealthStatus? Status => Check?.Status;

    private string DurationText => Check is null ? "–" : $"{Check.DurationMs:0} ms";

    private string DetailText => Check switch
    {
        null => "Unknown: the API runs this check and could not be reached",
        { Description: not null } => Check.Description,
        { Status: HealthStatus.Healthy } => "Responding normally",
        _ => "Not responding"
    };
}