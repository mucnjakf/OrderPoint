using System.Globalization;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace OrderPoint.Register.Orders.Dialogs;

// The bar's own terminal charges the card; the register only records it
public sealed partial class PayCardDialog
{
    private const string TotalStyle = "background-color: rgba(var(--mud-palette-text-primary-rgb), 0.04);";

    [Parameter]
    public decimal Total { get; set; }

    [CascadingParameter]
    private IMudDialogInstance MudDialogInstance { get; set; } = null!;

    private IReadOnlyList<(int Number, string Icon, string Text)> Steps =>
    [
        (1, Icons.Material.Filled.Dialpad,
            $"Enter {Total.ToString("C", CultureInfo.CurrentCulture)} on the card terminal"),
        (2, Icons.Material.Filled.Contactless, "Let the guest tap or insert the card"),
        (3, Icons.Material.Filled.TaskAlt, "Confirm here once the terminal says approved")
    ];

    private void Confirm()
    {
        MudDialogInstance.Close(DialogResult.Ok(true));
    }

    private void Cancel()
    {
        MudDialogInstance.Cancel();
    }
}