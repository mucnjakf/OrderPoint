using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace OrderPoint.Bar.Shift.Components;

// One figure of the shift; with OnClick it opens the matching orders
public sealed partial class ShiftTile
{
    [Parameter]
    [EditorRequired]
    public string Label { get; set; } = null!;

    [Parameter]
    [EditorRequired]
    public string Icon { get; set; } = null!;

    [Parameter]
    [EditorRequired]
    public Color Color { get; set; }

    [Parameter]
    [EditorRequired]
    public double? Value { get; set; }

    [Parameter]
    [EditorRequired]
    public string Text { get; set; } = null!;

    [Parameter]
    [EditorRequired]
    public string Caption { get; set; } = null!;

    [Parameter]
    public EventCallback OnClick { get; set; }

    private string PaperClass => OnClick.HasDelegate ? "pa-4 cursor-pointer" : "pa-4";

    private async Task OnTileClickAsync()
    {
        await OnClick.InvokeAsync();
    }
}