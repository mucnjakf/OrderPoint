using Microsoft.AspNetCore.Components;
using OrderPoint.Admin.Stock.Enumerations;
using OrderPoint.Admin.StockCounts.Dtos;
using OrderPoint.Shared.Extensions;
using OrderPoint.Shared.Services;

namespace OrderPoint.Admin.StockCounts.Components;

// One closed count: when, what it covered, how much was counted and what it found
public sealed partial class StockCountRow
{
    private static readonly int AllGroupsCount = Enum.GetValues<StockGroup>().Length;

    [Inject]
    private TimeZoneService TimeZoneService { get; set; } = null!;

    [Parameter]
    [EditorRequired]
    public StockCountDto StockCount { get; set; } = null!;

    // "Started Today 22:10 · Month end"
    private string NoteText
    {
        get
        {
            string started = $"Started {StockCount.CreatedAtUtc.ToDisplayDateTime(TimeZoneService.TimeZone)}";

            return StockCount.Note is null ? started : $"{started} · {StockCount.Note}";
        }
    }
}