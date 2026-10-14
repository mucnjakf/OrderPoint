using Microsoft.AspNetCore.Components;
using OrderPoint.Shared.Orders.Dtos;
using OrderPoint.Shared.Tables.Dtos;

namespace OrderPoint.Bartender.Tables.Components;

// The tapped table, what its guests asked for, and its open orders, oldest first; a ticket opens the order
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

    // What the guests at the table asked for, above the orders
    [Parameter]
    [EditorRequired]
    public IReadOnlyList<TableRequestDto> Requests { get; set; } = [];

    [Parameter]
    [EditorRequired]
    public Guid? CompletingRequestId { get; set; }

    [Parameter]
    [EditorRequired]
    public EventCallback<TableRequestDto> OnRequestDone { get; set; }

    [Parameter]
    [EditorRequired]
    public EventCallback OnClose { get; set; }
}