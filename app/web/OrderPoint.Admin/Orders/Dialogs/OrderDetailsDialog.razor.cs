using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Admin.Orders.Dtos;

namespace OrderPoint.Admin.Orders.Dialogs;

public sealed partial class OrderDetailsDialog
{
    [Parameter]
    public OrderDto Order { get; set; } = null!;

    [CascadingParameter]
    private IMudDialogInstance MudDialogInstance { get; set; } = null!;

    private static decimal GetSubtotal(OrderItemDto orderItem)
    {
        return orderItem.Quantity * orderItem.UnitPrice;
    }

    private void Close()
    {
        MudDialogInstance.Close(DialogResult.Ok(true));
    }
}