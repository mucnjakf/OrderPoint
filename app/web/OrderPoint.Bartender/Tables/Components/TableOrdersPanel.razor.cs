using Microsoft.AspNetCore.Components;
using OrderPoint.Shared.Orders.Dtos;
using OrderPoint.Shared.Tables.Dtos;

namespace OrderPoint.Bartender.Tables.Components;

// The tapped table and its open orders, oldest first; a ticket opens the order
public sealed partial class TableOrdersPanel
{
    [Parameter]
    [EditorRequired]
    public TableDto Table { get; set; } = null!;

    [Parameter]
    [EditorRequired]
    public string AreaName { get; set; } = null!;

    [Parameter]
    [EditorRequired]
    public IReadOnlyList<OrderDto> Orders { get; set; } = [];

    [Parameter]
    [EditorRequired]
    public bool IsLoading { get; set; }

    [Parameter]
    [EditorRequired]
    public EventCallback<OrderDto> OnOrderClick { get; set; }

    [Parameter]
    [EditorRequired]
    public EventCallback OnClose { get; set; }
}