using Microsoft.AspNetCore.Components;
using OrderPoint.Admin.Items.Dtos;

namespace OrderPoint.Admin.Items.Pages.Details.Components;

public sealed partial class ItemDetailsPaper
{
    [Parameter]
    [EditorRequired]
    public ItemDto Item { get; set; }

    [Parameter]
    public EventCallback OnDeleteClick { get; set; }

    private async Task OnDeleteClickedAsync()
    {
        await OnDeleteClick.InvokeAsync();
    }
}