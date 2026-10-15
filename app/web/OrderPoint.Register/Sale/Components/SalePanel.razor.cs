using System.Globalization;
using Microsoft.AspNetCore.Components;
using OrderPoint.Register.Sale.Dtos;
using OrderPoint.Register.Sale.Services;
using OrderPoint.Register.Shared.Layout;
using OrderPoint.Shared.Menu.Dtos;

namespace OrderPoint.Register.Sale.Components;

// The sale under the cards: its lines on the left, name, note, total and the payment buttons on the right;
// it sticks to the bottom of the screen while the cards scroll above it
public sealed partial class SalePanel
{
    // As long as the API accepts
    private const int NoteMaxLength = 200;

    private const int CustomerNameMaxLength = 30;

    // At the bottom of the screen: pushed there on a short page (the page is a flex column of the screen's height),
    // stuck there while the cards scroll; a solid bar over the page padding, so nothing shows at its sides or below it
    private const string PanelStyle =
        $"position: sticky; bottom: 0; z-index: 2; margin: auto calc(-1 * {PageGutter.Width}) -12px; " +
        "border-top: 1px solid var(--mud-palette-lines-default);";

    private const string PanelGridStyle =
        "display: grid; grid-template-columns: minmax(0, 1fr) 380px; gap: 24px;";

    // About five lines, then the lines scroll inside the panel; the gutter keeps the scroll bar off the subtotals
    private const string LinesStyle =
        "max-height: 240px; overflow-y: auto; scrollbar-gutter: stable; padding-right: 12px;";

    // The same gutter, so the column titles stay above their columns
    private const string HeaderStyle = "overflow-y: hidden; scrollbar-gutter: stable; padding-right: 12px;";

    // Item, price for one, quantity, subtotal
    private const string LineGridStyle =
        "display: grid; grid-template-columns: minmax(0, 1fr) 80px 128px 88px; gap: 12px; align-items: center;";

    private const string SecondaryStyle = "color: var(--mud-palette-text-secondary)";

    private const string GoneStyle = "color: var(--mud-palette-error-text)";

    [Parameter]
    [EditorRequired]
    public IReadOnlyList<(SaleLineDto Line, MenuItemDto? Item)> Rows { get; set; } = [];

    [Parameter]
    [EditorRequired]
    public int ItemsCount { get; set; }

    [Parameter]
    [EditorRequired]
    public decimal Total { get; set; }

    [Parameter]
    [EditorRequired]
    public string? CustomerName { get; set; }

    [Parameter]
    [EditorRequired]
    public EventCallback<string?> CustomerNameChanged { get; set; }

    [Parameter]
    [EditorRequired]
    public string? Note { get; set; }

    [Parameter]
    [EditorRequired]
    public EventCallback<string?> NoteChanged { get; set; }

    [Parameter]
    [EditorRequired]
    public bool IsPaying { get; set; }

    [Parameter]
    [EditorRequired]
    public EventCallback<(SaleLineDto Line, int Quantity)> OnQuantityChanged { get; set; }

    [Parameter]
    [EditorRequired]
    public EventCallback OnClear { get; set; }

    [Parameter]
    [EditorRequired]
    public EventCallback OnPayInCash { get; set; }

    [Parameter]
    [EditorRequired]
    public EventCallback OnPayByCard { get; set; }

    private bool HasUnavailableLines => Rows.Any(row => row.Item is null);

    private bool CanPay => !IsPaying && Rows.Count != 0 && !HasUnavailableLines;

    private static string GetSubtotalText(MenuItemDto item, SaleLineDto line)
    {
        return (item.Price * line.Quantity).ToString("C", CultureInfo.CurrentCulture);
    }

    private async Task OnQuantityChangedAsync(SaleLineDto line, int quantity)
    {
        await OnQuantityChanged.InvokeAsync((line, quantity));
    }

    private async Task OnCustomerNameChanged(string? customerName)
    {
        await CustomerNameChanged.InvokeAsync(customerName);
    }

    private async Task OnNoteChanged(string? note)
    {
        await NoteChanged.InvokeAsync(note);
    }
}