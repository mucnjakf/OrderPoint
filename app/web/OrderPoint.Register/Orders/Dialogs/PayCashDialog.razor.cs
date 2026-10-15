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

    private const string TotalStyle = "background-color: rgba(var(--mud-palette-text-primary-rgb), 0.04);";

    private const string QuickAmountsStyle = "display: grid; grid-template-columns: repeat(4, 1fr); gap: 8px;";

    private const string ChangeStyle = "background-color: rgba(var(--mud-palette-success-rgb), 0.12);";

    private const string ShortStyle = "background-color: rgba(var(--mud-palette-error-rgb), 0.12);";

    private const string WaitingStyle = "background-color: rgba(var(--mud-palette-text-primary-rgb), 0.04);";

    private static readonly decimal[] NoteSteps = [5, 10, 20, 50, 100];

    [Parameter]
    public decimal Total { get; set; }

    [CascadingParameter]
    private IMudDialogInstance MudDialogInstance { get; set; } = null!;

    private decimal? Received { get; set; }

    private bool IsEnough => Received is { } received && received >= Total && received <= MaxCashReceived;

    private bool IsShort => Received is { } received && received < Total;

    // The exact amount, then the total rounded up to the next notes
    private IEnumerable<decimal> QuickAmounts => NoteSteps
        .Select(step => Math.Ceiling(Total / step) * step)
        .Prepend(Total)
        .Distinct()
        .Take(QuickAmountsCount);

    private string ChangeLabel => IsShort ? "Still short" : "Change to give";

    private string ChangeText => Received switch
    {
        null => "-",
        { } received when received < Total => (Total - received).ToString("C", CultureInfo.CurrentCulture),
        { } received => (received - Total).ToString("C", CultureInfo.CurrentCulture)
    };

    private string ChangeIcon => IsShort ? Icons.Material.Filled.ErrorOutline : Icons.Material.Filled.CurrencyExchange;

    private Color ChangeColor => Received is null ? Color.Default : IsShort ? Color.Error : Color.Success;

    private string ChangeBoxStyle => Received is null ? WaitingStyle : IsShort ? ShortStyle : ChangeStyle;

    private string ConfirmText => Received is { } received && IsEnough
        ? $"Take {received.ToString("C", CultureInfo.CurrentCulture)}"
        : "Take cash";

    private void Confirm()
    {
        MudDialogInstance.Close(DialogResult.Ok(Received!.Value));
    }

    private void Cancel()
    {
        MudDialogInstance.Cancel();
    }
}