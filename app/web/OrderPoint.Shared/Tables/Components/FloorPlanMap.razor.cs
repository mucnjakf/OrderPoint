using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using OrderPoint.Shared.Extensions;
using OrderPoint.Shared.Tables.Dtos;
using OrderPoint.Shared.Tables.Enumerations;

namespace OrderPoint.Shared.Tables.Components;

// An area's floor plan as a grid of cells; editable, its pieces can be dragged and snap to the cells
public sealed partial class FloorPlanMap : IAsyncDisposable
{
    private const string ModulePath = "./_content/OrderPoint.Shared/Tables/Components/FloorPlanMap.razor.js";

    private const string PieceTextStyle =
        "line-height: 1.2; text-align: center; max-inline-size: 100%; overflow: hidden; text-overflow: ellipsis; " +
        "white-space: nowrap; padding: 0 2px;";

    // A piece taller than wide (rotated) reads along its length, like the floor plan turned with it
    private const string VerticalStyle = "writing-mode: vertical-rl;";

    private const string VerticalIconStyle = "transform: rotate(90deg);";

    private const string PieceStyle =
        "display: flex; flex-direction: column; align-items: center; justify-content: center; overflow: hidden; " +
        "min-width: 0; min-height: 0;";

    private const string SelectedStyle = "outline: 3px solid var(--mud-palette-primary); outline-offset: 2px;";

    private IJSObjectReference? _module;

    private IJSObjectReference? _dragging;

    private DotNetObjectReference<FloorPlanMap>? _reference;

    [Inject]
    private IJSRuntime JsRuntime { get; set; } = null!;

    [Parameter]
    [EditorRequired]
    public AreaDto Area { get; set; } = null!;

    [Parameter]
    public Guid? SelectedId { get; set; }

    // Tables and fixtures can be dragged; a drop is only kept when OnMoveAsync accepts it
    [Parameter]
    public bool IsEditable { get; set; }

    // A table's or fixture's id, or null for a click on the empty floor
    [Parameter]
    public EventCallback<Guid?> OnSelect { get; set; }

    [Parameter]
    public Func<FloorPlanMoveDto, Task<bool>>? OnMoveAsync { get; set; }

    private ElementReference MapElement { get; set; }

    // The grid lines are a background, one cell per column and row
    private string MapStyle =>
        $"display: grid; grid-template-columns: repeat({Area.Columns}, 1fr); " +
        $"grid-template-rows: repeat({Area.Rows}, 1fr); aspect-ratio: {Area.Columns} / {Area.Rows}; " +
        "background-image: linear-gradient(to right, var(--mud-palette-lines-default) 1px, transparent 1px), " +
        "linear-gradient(to bottom, var(--mud-palette-lines-default) 1px, transparent 1px); " +
        $"background-size: calc(100% / {Area.Columns}) calc(100% / {Area.Rows}); " +
        "border: 1px solid var(--mud-palette-lines-default); border-radius: var(--mud-default-borderradius); " +
        "user-select: none; position: relative;";

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender || !IsEditable)
        {
            return;
        }

        _module = await JsRuntime.InvokeAsync<IJSObjectReference>("import", ModulePath);
        _reference = DotNetObjectReference.Create(this);
        _dragging = await _module.InvokeAsync<IJSObjectReference>("enableDragging", MapElement, _reference);
    }

    [JSInvokable]
    public async Task<bool> OnPieceDroppedAsync(Guid id, int x, int y)
    {
        if (OnMoveAsync is null)
        {
            return false;
        }

        return await OnMoveAsync(new FloorPlanMoveDto(id, x, y));
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            if (_dragging is not null)
            {
                await _dragging.InvokeVoidAsync("dispose");
                await _dragging.DisposeAsync();
            }

            if (_module is not null)
            {
                await _module.DisposeAsync();
            }
        }
        catch (JSDisconnectedException)
        {
            // The browser is already gone, and the module with it
        }

        _reference?.Dispose();
    }

    private async Task SelectAsync(Guid id)
    {
        await OnSelect.InvokeAsync(id);
    }

    private async Task ClearSelectionAsync()
    {
        await OnSelect.InvokeAsync(null);
    }

    private string GetTableStyle(TableDto table)
    {
        string borderRadius = table.Shape is TableShape.Round ? "50%" : "var(--mud-default-borderradius)";
        string borderStyle = table.Status is TableStatus.Active ? "solid" : "dashed";
        string opacity = table.Status is TableStatus.Active ? "1" : "0.5";

        return $"{GetPlacementStyle(table.Id, table.X, table.Y, table.Width, table.Height)} {PieceStyle} " +
               $"margin: 6px; border: 2px {borderStyle} var(--mud-palette-text-primary); " +
               $"border-radius: {borderRadius}; background-color: var(--mud-palette-surface); opacity: {opacity};";
    }

    private string GetFixtureStyle(FixtureDto fixture)
    {
        string background = fixture.Type switch
        {
            FixtureType.Wall => "rgba(var(--mud-palette-text-primary-rgb), 0.6)",
            FixtureType.BarCounter => "rgba(var(--mud-palette-text-primary-rgb), 0.16)",
            _ => "transparent"
        };

        string border = fixture.Type is FixtureType.Entrance
            ? "2px dashed var(--mud-palette-text-secondary)"
            : "none";

        return $"{GetPlacementStyle(fixture.Id, fixture.X, fixture.Y, fixture.Width, fixture.Height)} {PieceStyle} " +
               $"margin: 2px; background-color: {background}; border: {border}; " +
               "border-radius: var(--mud-default-borderradius); color: var(--mud-palette-text-secondary);";
    }

    private string GetPlacementStyle(Guid id, int x, int y, int width, int height)
    {
        string cursor = IsEditable ? "cursor: grab; touch-action: none;" : "cursor: pointer;";
        string selected = id == SelectedId ? SelectedStyle : string.Empty;
        string vertical = height > width ? VerticalStyle : string.Empty;

        return $"grid-column: {x + 1} / span {width}; grid-row: {y + 1} / span {height}; " +
               $"{cursor} {selected} {vertical}";
    }

    private static string? GetFixtureIconStyle(FixtureDto fixture)
    {
        return fixture.Height > fixture.Width ? VerticalIconStyle : null;
    }

    private static string GetFixtureLabel(FixtureDto fixture)
    {
        if (!string.IsNullOrWhiteSpace(fixture.Label))
        {
            return fixture.Label;
        }

        // A wall needs no words
        return fixture.Type is FixtureType.Wall ? string.Empty : fixture.Type.GetLabel();
    }

    private static string? GetFixtureIcon(FixtureDto fixture)
    {
        return fixture.Type switch
        {
            FixtureType.BarCounter => Icons.Material.Filled.LocalBar,
            FixtureType.Entrance => Icons.Material.Filled.MeetingRoom,
            _ => null
        };
    }
}