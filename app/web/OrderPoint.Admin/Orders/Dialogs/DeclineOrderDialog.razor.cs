using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Shared.Orders.Dtos;

namespace OrderPoint.Admin.Orders.Dialogs;

// For an open order nobody will finish, e.g. at closing time
public sealed partial class DeclineOrderDialog
{
    [Parameter]
    [EditorRequired]
    public OrderDto Order { get; set; } = null!;

    [CascadingParameter]
    private IMudDialogInstance MudDialogInstance { get; set; } = null!;

    private string HandlerText => Order.Bartender is null
        ? "Not taken by a bartender yet"
        : $"Taken by {Order.Bartender.FirstName} {Order.Bartender.LastName}";

    private void Confirm()
    {
        MudDialogInstance.Close(DialogResult.Ok(true));
    }

    private void Cancel()
    {
        MudDialogInstance.Cancel();
    }
}