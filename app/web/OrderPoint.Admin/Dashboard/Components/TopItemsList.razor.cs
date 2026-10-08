using Microsoft.AspNetCore.Components;
using OrderPoint.Admin.Dashboard.Dtos;

namespace OrderPoint.Admin.Dashboard.Components;

public sealed partial class TopItemsList
{
    [Parameter]
    [EditorRequired]
    public IReadOnlyList<TopItemDto> Items { get; set; } = [];

    private int TopQuantity => Items.Count == 0 ? 0 : Items.Max(item => item.Quantity);

    private double GetShareOfTop(TopItemDto item)
    {
        return TopQuantity == 0 ? 0 : item.Quantity * 100.0 / TopQuantity;
    }
}