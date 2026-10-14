using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Admin.Orders.Api.Requests;
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

    private DeclineOrderRequest Request { get; set; } = new();

    private string HandlerText => Order.Bartender is null
        ? "Not taken by a bartender yet"
        : $"Taken by {Order.Bartender.FirstName} {Order.Bartender.LastName}";

    private void OnValidSubmit()
    {
        MudDialogInstance.Close(DialogResult.Ok(Request));
    }

    private void Cancel()
    {
        MudDialogInstance.Cancel();
    }
}