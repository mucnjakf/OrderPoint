using Microsoft.AspNetCore.Components;
using OrderPoint.Shared.Extensions;
using OrderPoint.Shared.Tables.Dtos;
using OrderPoint.Shared.Tables.Enumerations;

namespace OrderPoint.Admin.Tables.Components;

// Shown while nothing is selected: the area itself and its tables, a click selects one
public sealed partial class AreaPanel
{
    private const string HoveredRowStyle = "background-color: var(--mud-palette-table-hover);";

    [Parameter]
    [EditorRequired]
    public AreaDto Area { get; set; } = null!;

    [Parameter]
    [EditorRequired]
    public EventCallback<TableDto> OnTableClick { get; set; }

    [Parameter]
    [EditorRequired]
    public EventCallback OnEdit { get; set; }

    [Parameter]
    [EditorRequired]
    public EventCallback OnDelete { get; set; }

    private Guid? HoveredTableId { get; set; }

    private string AreaSummaryText
    {
        get
        {
            int seatsCount = Area.Tables.Sum(table => table.Seats);
            int inactiveCount = Area.Tables.Count(table => table.Status is TableStatus.Inactive);

            string text = $"{Area.Tables.Count} · {seatsCount.ToCountText("seat", "seats")}";

            return inactiveCount == 0 ? text : $"{text} · {inactiveCount} inactive";
        }
    }
}