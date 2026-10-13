using Microsoft.AspNetCore.Components;

namespace OrderPoint.Shared.Components;

public sealed partial class NewItemHighlight
{
    [Parameter]
    [EditorRequired]
    public bool IsNew { get; set; }

    [Parameter]
    [EditorRequired]
    public RenderFragment ChildContent { get; set; } = null!;
}