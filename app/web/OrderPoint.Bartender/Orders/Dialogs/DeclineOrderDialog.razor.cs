using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Bartender.Orders.Api.Requests;

namespace OrderPoint.Bartender.Orders.Dialogs;

public sealed partial class DeclineOrderDialog
{
    [Parameter]
    public int OrderNumber { get; set; }

    [CascadingParameter]
    private IMudDialogInstance MudDialogInstance { get; set; } = null!;

    private DeclineOrderRequest Request { get; set; } = new();

    private void OnValidSubmit()
    {
        MudDialogInstance.Close(DialogResult.Ok(Request));
    }

    private void Cancel()
    {
        MudDialogInstance.Cancel();
    }
}