using Microsoft.AspNetCore.Components;
using OrderPoint.Admin.Dashboard.Dtos;

namespace OrderPoint.Admin.Dashboard.Components;

public sealed partial class TopItemsList
{
    [Parameter]
    [EditorRequired]
    public IReadOnlyList<TopItemDto> Items { get; set; } = [];

    [Parameter]
    [EditorRequired]
    public EventCallback<TopItemDto> OnItemClick { get; set; }

    private Guid? HoveredItemId { get; set; }

    private int TopQuantity => Items.Count == 0 ? 0 : Items.Max(item => item.Quantity);

    private string? GetRowStyle(TopItemDto item)
    {
        return item.ItemId == HoveredItemId ? "background-color: var(--mud-palette-table-hover);" : null;
    }

    private async Task OnItemClickAsync(TopItemDto item)
    {
        await OnItemClick.InvokeAsync(item);
    }

    private double GetShareOfTop(TopItemDto item)
    {
        return TopQuantity == 0 ? 0 : item.Quantity * 100.0 / TopQuantity;
    }
}