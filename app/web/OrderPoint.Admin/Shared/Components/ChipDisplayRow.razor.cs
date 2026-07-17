using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace OrderPoint.Admin.Shared.Components;

public sealed partial class ChipDisplayRow
{
    [Parameter]
    [EditorRequired]
    public string Icon { get; set; }

    [Parameter]
    [EditorRequired]
    public string Label { get; set; }

    [Parameter]
    [EditorRequired]
    public string Text { get; set; }

    [Parameter]
    [EditorRequired]
    public Color Color { get; set; }

    [Parameter]
    public bool ShowDivider { get; set; }
}