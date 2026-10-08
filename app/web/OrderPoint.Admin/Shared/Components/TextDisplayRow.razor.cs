using Microsoft.AspNetCore.Components;

namespace OrderPoint.Admin.Shared.Components;

public sealed partial class TextDisplayRow
{
    [Parameter]
    [EditorRequired]
    public string Icon { get; set; }

    [Parameter]
    [EditorRequired]
    public string Label { get; set; }

    [Parameter]
    public string? Text { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public bool ShowDivider { get; set; }
}