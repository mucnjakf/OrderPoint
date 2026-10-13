using Microsoft.AspNetCore.Components;

namespace OrderPoint.Shared.Components;

public sealed partial class StatefulView
{
    [Parameter]
    [EditorRequired]
    public bool IsLoading { get; set; }

    [Parameter]
    [EditorRequired]
    public bool ShowContent { get; set; }

    [Parameter]
    public string? EmptyStateIcon { get; set; }

    [Parameter]
    public string? EmptyStateText { get; set; }

    // A second, quieter line under the empty state text (full-size states only)
    [Parameter]
    public string? EmptyStateDetail { get; set; }

    [Parameter]
    public bool Compact { get; set; }

    // Inside a framed list: the full-size states without a paper of their own, centred in the held height
    [Parameter]
    public bool Flat { get; set; }

    [Parameter]
    public RenderFragment? LoadingContent { get; set; }

    [Parameter]
    [EditorRequired]
    public RenderFragment ChildContent { get; set; } = null!;
}