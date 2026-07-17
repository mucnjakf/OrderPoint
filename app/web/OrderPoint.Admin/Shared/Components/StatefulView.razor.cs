using Microsoft.AspNetCore.Components;

namespace OrderPoint.Admin.Shared.Components;

public sealed partial class StatefulView
{
    [Parameter]
    [EditorRequired]
    public bool IsLoading { get; set; }

    [Parameter]
    [EditorRequired]
    public bool ShowContent { get; set; }

    [Parameter]
    [EditorRequired]
    public string EmptyStateIcon { get; set; } = null!;

    [Parameter]
    [EditorRequired]
    public string EmptyStateText { get; set; } = null!;

    [Parameter]
    [EditorRequired]
    public RenderFragment ChildContent { get; set; } = null!;
}