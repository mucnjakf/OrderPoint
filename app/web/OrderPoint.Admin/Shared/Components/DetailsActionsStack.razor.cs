using Microsoft.AspNetCore.Components;

namespace OrderPoint.Admin.Shared.Components;

public sealed partial class DetailsActionsStack
{
    [Parameter]
    [EditorRequired]
    public string UpdateButtonHref { get; set; }

    [Parameter]
    public bool DeleteButtonDisabled { get; set; }

    [Parameter]
    public string? DeleteButtonDisabledTooltipText { get; set; }

    [Parameter]
    public EventCallback OnDeleteClick { get; set; }

    private async Task OnDeleteClickAsync()
    {
        await OnDeleteClick.InvokeAsync();
    }
}