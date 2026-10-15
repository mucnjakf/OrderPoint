using System.Globalization;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace OrderPoint.Register.Orders.Dialogs;

// What the guest hands over, with the usual notes as quick picks, and the change to give back
public sealed partial class PayCashDialog
{
    // As much as the API accepts, so a typo like 2000 for 20.00 is caught
    private const decimal MaxCashReceived = 1000;

    private const int QuickAmountsCount = 4;

    private const string NotEnoughStyle = "color: var(--mud-palette-error)";

    private static readonly decimal[] NoteSteps = [5, 10, 20, 50, 100];

    [Parameter]
    public decimal Total { get; set; }

    [CascadingParameter]
    private IMudDialogInstance MudDialogInstance { get; set; } = null!;

    private decimal? Received { get; set; }

    private bool IsEnough => Received is { } received && received >= Total && received <= MaxCashReceived;

    // The exact amount, then the total rounded up to the next notes
    private IEnumerable<decimal> QuickAmounts => NoteSteps
        .Select(step => Math.Ceiling(Total / step) * step)
        .Prepend(Total)
        .Distinct()
        .Take(QuickAmountsCount);

    private string ChangeText => Received is { } received && received >= Total
        ? (received - Total).ToString("C", CultureInfo.CurrentCulture)
        : "-";

    private string? ChangeStyle => Received is { } received && received < Total ? NotEnoughStyle : null;

    private void Confirm()
    {
        MudDialogInstance.Close(DialogResult.Ok(Received!.Value));
    }

    private void Cancel()
    {
        MudDialogInstance.Cancel();
    }
}