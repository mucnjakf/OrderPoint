using Microsoft.AspNetCore.Components;
using OrderPoint.Shared.Extensions;
using OrderPoint.Shared.Tables.Dtos;
using OrderPoint.Shared.Tables.Enumerations;

namespace OrderPoint.Bartender.Tables.Components;

// A table to take an order at; a bell when its guests call, faded and not tappable while it is closed
public sealed partial class TableTile
{
    private const string TileStyle = "position: relative; text-align: center;";

    private const string ClosedTileStyle = $"{TileStyle} opacity: 0.5; border-style: dashed;";

    [Parameter]
    [EditorRequired]
    public TableDto Table { get; set; } = null!;

    [Parameter]
    [EditorRequired]
    public bool HasCall { get; set; }

    [Parameter]
    [EditorRequired]
    public EventCallback<TableDto> OnClick { get; set; }

    private bool IsClosed => Table.Status == TableStatus.Inactive;

    private string PaperClass => IsClosed ? "pa-3" : "pa-3 cursor-pointer";

    private string PaperStyle => IsClosed ? ClosedTileStyle : TileStyle;

    private string SubtitleText => IsClosed ? "Closed" : Table.Seats.ToCountText("seat", "seats");

    private async Task OnTileClickAsync()
    {
        if (IsClosed)
        {
            return;
        }

        await OnClick.InvokeAsync(Table);
    }
}