using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace OrderPoint.Shared.Components;

// − n +; with Min 0 the minus becomes a bin at 1, because going lower removes the line
public sealed partial class QuantityStepper
{
    [Parameter]
    [EditorRequired]
    public int Value { get; set; }

    [Parameter]
    [EditorRequired]
    public EventCallback<int> ValueChanged { get; set; }

    [Parameter]
    public int Min { get; set; } = 1;

    [Parameter]
    [EditorRequired]
    public int Max { get; set; }

    private string DecreaseIcon => Min == 0 && Value == 1
        ? Icons.Material.Filled.DeleteOutline
        : Icons.Material.Filled.Remove;

    private async Task ChangeAsync(int value)
    {
        await ValueChanged.InvokeAsync(Math.Clamp(value, Min, Max));
    }
}