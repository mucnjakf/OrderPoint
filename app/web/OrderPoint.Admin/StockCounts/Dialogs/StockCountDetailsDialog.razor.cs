using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Admin.Shared.Extensions;
using OrderPoint.Admin.Stock.Enumerations;
using OrderPoint.Admin.StockCounts.Dtos;
using OrderPoint.Shared.Extensions;
using OrderPoint.Shared.Services;

namespace OrderPoint.Admin.StockCounts.Dialogs;

// A closed count: what it found, the biggest loss first
public sealed partial class StockCountDetailsDialog
{
    private const string FactsStyle = "display: grid; grid-template-columns: repeat(4, minmax(0, 1fr)); gap: 12px;";

    private const string NotCountedStyle = "color: var(--mud-palette-text-disabled)";

    private static readonly int AllGroupsCount = Enum.GetValues<StockGroup>().Length;

    [Inject]
    private TimeZoneService TimeZoneService { get; set; } = null!;

    [Parameter]
    public StockCountDto StockCount { get; set; } = null!;

    [CascadingParameter]
    private IMudDialogInstance MudDialogInstance { get; set; } = null!;

    // "All groups · started Today 22:10 · Month end"
    private string SubtitleText
    {
        get
        {
            string text = $"{StockCount.GetGroupsText(AllGroupsCount)} · started " +
                          StockCount.CreatedAtUtc.ToDisplayDateTime(TimeZoneService.TimeZone);

            return StockCount.Note is null ? text : $"{text} · {StockCount.Note}";
        }
    }

    private IReadOnlyList<StockCountLineDto> Lines => StockCount.Lines
        .OrderBy(line => line.CountedQuantity is null)
        .ThenBy(line => line.ValueChange ?? 0)
        .ThenBy(line => line.StockItemName)
        .ToList();

    private decimal Losses => StockCount.Lines.Where(line => line.ValueChange < 0).Sum(line => line.ValueChange!.Value);

    private decimal Gains => StockCount.Lines.Where(line => line.ValueChange > 0).Sum(line => line.ValueChange!.Value);

    private IReadOnlyList<(string Icon, string Label, string Value, string? Style)> Facts =>
    [
        (Icons.Material.Filled.Checklist, "Counted", $"{StockCount.CountedCount} of {StockCount.LinesCount}", null),
        (Icons.Material.Filled.Euro, "Difference", StockCount.ValueChange.ToSignedMoneyText(),
            StockCount.ValueChange.GetValueChangeStyle()),
        (Icons.Material.Filled.TrendingDown, "Losses", Losses.ToSignedMoneyText(), Losses.GetValueChangeStyle()),
        (Icons.Material.Filled.TrendingUp, "Gains", Gains.ToSignedMoneyText(), Gains.GetValueChangeStyle())
    ];

    private void Close()
    {
        MudDialogInstance.Close();
    }
}