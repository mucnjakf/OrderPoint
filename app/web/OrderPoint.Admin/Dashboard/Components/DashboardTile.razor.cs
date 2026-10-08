using Microsoft.AspNetCore.Components;

namespace OrderPoint.Admin.Dashboard.Components;

public sealed partial class DashboardTile
{
    [Parameter]
    [EditorRequired]
    public string Title { get; set; }

    [Parameter]
    [EditorRequired]
    public string Icon { get; set; }

    [Parameter]
    [EditorRequired]
    public bool IsLoading { get; set; }

    [Parameter]
    [EditorRequired]
    public bool ShowContent { get; set; }

    [Parameter]
    [EditorRequired]
    public string EmptyStateText { get; set; }

    [Parameter]
    public RenderFragment? HeaderContent { get; set; }

    [Parameter]
    [EditorRequired]
    public RenderFragment ChildContent { get; set; } = null!;
}