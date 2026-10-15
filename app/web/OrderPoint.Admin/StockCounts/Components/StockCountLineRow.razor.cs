using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Admin.Shared.Extensions;
using OrderPoint.Admin.StockCounts.Dtos;

namespace OrderPoint.Admin.StockCounts.Components;

// One stock item on the count sheet: what the system expects, full packs plus what is left in opened ones, and the
// difference once counted
public sealed partial class StockCountLineRow
{
    [Parameter]
    [EditorRequired]
    public StockCountEntryDto Entry { get; set; } = null!;

    [Parameter]
    public bool IsReadOnly { get; set; }

    // Raised when a field was left with a new value; the page saves it
    [Parameter]
    [EditorRequired]
    public EventCallback<StockCountEntryDto> OnChanged { get; set; }

    private StockCountLineDto Line => Entry.Line;

    // A pack of one base unit (a single piece) has nothing opened
    private bool HasOpenedAmount => Line.PackSize != 1;

    // "bottles"
    private string PacksLabel => Line.PackName.ToPluralPackName();

    private string ExpectedText => Line.ExpectedQuantity is { } expected ? Line.GetQuantityText(expected) : "-";

    private string StatusIcon => Line.CountedQuantity is null
        ? Icons.Material.Filled.RadioButtonUnchecked
        : Icons.Material.Filled.CheckCircle;

    private Color StatusColor => Line.CountedQuantity is null ? Color.Default : Color.Success;

    private string ChangeStyle => Line.QuantityChange is null or 0
        ? "color: var(--mud-palette-text-secondary)"
        : "font-weight: 600";

    private async Task OnChangedAsync()
    {
        await OnChanged.InvokeAsync(Entry);
    }

    private async Task ClearAsync()
    {
        Entry.FullPacks = null;
        Entry.OpenedAmount = null;

        await OnChanged.InvokeAsync(Entry);
    }
}