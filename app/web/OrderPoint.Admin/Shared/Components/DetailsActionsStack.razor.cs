using Microsoft.AspNetCore.Components;

namespace OrderPoint.Admin.Shared.Components;

public sealed partial class DetailsActionsStack
{
    [Parameter]
    public EventCallback OnUpdateClick { get; set; }

    [Parameter]
    public bool DeleteButtonDisabled { get; set; }

    [Parameter]
    public string? DeleteButtonDisabledTooltipText { get; set; }

    [Parameter]
    public EventCallback OnDeleteClick { get; set; }

    private async Task OnUpdateClickAsync()
    {
        await OnUpdateClick.InvokeAsync();
    }

    private async Task OnDeleteClickAsync()
    {
        await OnDeleteClick.InvokeAsync();
    }
}