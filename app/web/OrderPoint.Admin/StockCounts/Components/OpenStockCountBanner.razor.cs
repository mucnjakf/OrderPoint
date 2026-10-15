using Microsoft.AspNetCore.Components;
using OrderPoint.Admin.Shared.Extensions;
using OrderPoint.Admin.Stock.Enumerations;
using OrderPoint.Admin.StockCounts.Dtos;
using OrderPoint.Shared.Extensions;
using OrderPoint.Shared.Services;

namespace OrderPoint.Admin.StockCounts.Components;

// The count that is still open, on top of the Counts page
public sealed partial class OpenStockCountBanner
{
    private static readonly int AllGroupsCount = Enum.GetValues<StockGroup>().Length;

    [Inject]
    private TimeZoneService TimeZoneService { get; set; } = null!;

    [Parameter]
    [EditorRequired]
    public StockCountDto StockCount { get; set; } = null!;

    [Parameter]
    [EditorRequired]
    public EventCallback OnDiscardClick { get; set; }

    private double ProgressPercent => 100.0 * StockCount.CountedCount / StockCount.LinesCount;

    // "34 of 52 counted · started Today 20:10 · difference so far −€12.40"
    private string DetailText =>
        $"{StockCount.CountedCount} of {StockCount.LinesCount} counted · started " +
        $"{StockCount.CreatedAtUtc.ToDisplayDateTime(TimeZoneService.TimeZone)} · difference so far " +
        StockCount.ValueChange.ToSignedMoneyText();

    private async Task OnDiscardClickAsync()
    {
        await OnDiscardClick.InvokeAsync();
    }
}