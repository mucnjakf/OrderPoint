using Microsoft.AspNetCore.Components;
using OrderPoint.Bar.Shift.Dtos;

namespace OrderPoint.Bar.Shift.Components;

// What the bartender served today: totals and orders per hour next to the items they served most
public sealed partial class ShiftDay
{
    [Parameter]
    [EditorRequired]
    public ShiftDayDto Day { get; set; } = null!;

    private int TopQuantity => Day.TopItems.Count == 0 ? 0 : Day.TopItems.Max(item => item.Quantity);

    private double GetShareOfTop(TopItemDto item)
    {
        return TopQuantity == 0 ? 0 : item.Quantity * 100.0 / TopQuantity;
    }
}