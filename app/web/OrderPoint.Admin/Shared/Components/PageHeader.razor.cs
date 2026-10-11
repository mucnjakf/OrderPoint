using Microsoft.AspNetCore.Components;

namespace OrderPoint.Admin.Shared.Components;

public sealed partial class PageHeader
{
    [Parameter]
    [EditorRequired]
    public string Text { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}