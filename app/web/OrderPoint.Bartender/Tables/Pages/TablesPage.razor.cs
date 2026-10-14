using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using OrderPoint.Bartender.Orders.Api;
using OrderPoint.Bartender.Orders.Services;
using OrderPoint.Bartender.Orders.Sorting;
using OrderPoint.Bartender.Shared.Layout;
using OrderPoint.Bartender.Tables.Api;
using OrderPoint.Bartender.Tables.Dtos;
using OrderPoint.Shared.Dtos;
using OrderPoint.Shared.Extensions;
using OrderPoint.Shared.Orders.Dtos;
using OrderPoint.Shared.Orders.Enumerations;
using OrderPoint.Shared.Orders.Services;
using OrderPoint.Shared.Services;
using OrderPoint.Shared.Tables.Dtos;

namespace OrderPoint.Bartender.Tables.Pages;

// Where each table is and which ones have open orders; a tapped table shows them, live
public sealed partial class TablesPage : IAsyncDisposable
{
    // Each open status in one call, like the Orders tabs; a table rarely has more
    private const int MaxOrdersPerStatus = 50;

    // The colours move on even when nothing changes
    private const int TimeRefreshIntervalInSeconds = 30;

    private const string WarningColor = "warning";

    private const string InfoColor = "info";

    private const string ErrorColor = "error";

    private const string AreaBarStyle = "overflow: hidden;";

    private const string ActiveAreaStyle = "border-radius: 0; border-bottom: 2px solid var(--mud-palette-primary);";

    private const string InactiveAreaStyle =
        "border-radius: 0; border-bottom: 2px solid transparent; color: var(--mud-palette-text-secondary);";

    // The page's one heading, for screen readers only: the map uses the whole screen
    private const string VisuallyHiddenStyle =
        "position: absolute; width: 1px; height: 1px; overflow: hidden; clip-path: inset(50%); white-space: nowrap;";

    // Phone: the tapped table's orders slide over the lower part of the screen, above the bottom navigation
    private const string SheetHeight = "50vh";

    // The screen between the dense app bar and the bottom navigation, minus the content's padding
    internal const string ScreenHeight = $"100dvh - 48px - {BottomNavigation.Height} - 24px";

    // Cells big enough to read and tap; a bigger floor plan is panned with a finger
    private const int MinCellSizeInPixels = 40;

    private const string MapFrameStyle =
        "position: relative; flex: 1; min-height: 0; border: 1px solid var(--mud-palette-lines-default); " +
        "border-radius: var(--mud-default-borderradius); overflow: hidden;";

    // The frame's whole area scrolls both ways; the legend stays outside it, in the frame's corner
    private const string MapScrollStyle = "position: absolute; inset: 0; overflow: auto; container-type: size;";

    private const string LegendStyle = "position: absolute; top: 8px; left: 8px; z-index: 1;";

    // Opposite the order count, so a table can show both
    private const string RequestBadgeStyle =
        "position: absolute; top: 2px; left: 2px; writing-mode: horizontal-tb; width: 18px; height: 18px; " +
        "border-radius: 50%; display: flex; align-items: center; justify-content: center; " +
        "background-color: var(--mud-palette-primary); color: var(--mud-palette-primary-text);";

    private const string SidePanelStyle = "flex: 5 1 0; min-width: 0; overflow-y: auto;";

    private const string BottomSheetStyle =
        $"position: fixed; left: 0; right: 0; bottom: {BottomNavigation.Height}; height: {SheetHeight}; " +
        "overflow-y: auto; z-index: var(--mud-zindex-appbar); " +
        "border-top: 1px solid var(--mud-palette-lines-default); border-radius: 12px 12px 0 0;";

    // Like the tickets: an open order turns red once it has waited this long
    private static readonly TimeSpan LongWait = TimeSpan.FromMinutes(10);

    private static readonly (string Label, string Color)[] Legend =
    [
        ("Waiting for a bartender", WarningColor),
        ("In progress", InfoColor),
        ("Waiting 10+ min", ErrorColor)
    ];

    private static readonly OrderStatus[] OpenStatuses =
        [OrderStatus.Pending, OrderStatus.Accepted, OrderStatus.Active];

    private readonly Guid _viewportSubscriptionId = Guid.NewGuid();

    private readonly PeriodicTimer _timeRefreshTimer = new(TimeSpan.FromSeconds(TimeRefreshIntervalInSeconds));

    private IJSObjectReference? _module;

    // Scrolled to after the next render, once the table (and on a phone the sheet) is on screen
    private Guid? _tableToScrollTo;

    [Inject]
    private ApiService ApiService { get; set; } = null!;

    [Inject]
    private TableApiClient TableApiClient { get; set; } = null!;

    [Inject]
    private OrderApiClient OrderApiClient { get; set; } = null!;

    [Inject]
    private OrdersHubService OrdersHubService { get; set; } = null!;

    [Inject]
    private OrdersTabState OrdersTabState { get; set; } = null!;

    [Inject]
    private IBrowserViewportService BrowserViewportService { get; set; } = null!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    [Inject]
    private IJSRuntime JsRuntime { get; set; } = null!;

    // "Show on map" from an order opens /tables?table=T7
    [SupplyParameterFromQuery(Name = "table")]
    public string? TableCode { get; set; }

    private IReadOnlyList<AreaDto> Areas { get; set; } = [];

    private bool IsLoadingFloorPlan { get; set; } = true;

    private Dictionary<Guid, TableStatusDto> StatusesByTableId { get; set; } = [];

    private Guid? SelectedAreaId { get; set; }

    private Guid? SelectedTableId { get; set; }

    private IReadOnlyList<OrderDto> TableOrders { get; set; } = [];

    private IReadOnlyList<TableRequestDto> Requests { get; set; } = [];

    private Guid? CompletingRequestId { get; set; }

    private bool IsLoadingTableOrders { get; set; }

    private bool IsSplitView { get; set; }

    private ElementReference MapScrollElement { get; set; }

    private AreaDto? SelectedArea =>
        Areas.FirstOrDefault(area => area.Id == SelectedAreaId) ?? Areas.FirstOrDefault();

    private TableDto? SelectedTable => SelectedArea?.Tables.FirstOrDefault(table => table.Id == SelectedTableId);

    private IReadOnlyList<TableRequestDto> SelectedTableRequests =>
        Requests.Where(request => request.TableId == SelectedTableId).ToList();

    private const string ModulePath = "./Tables/Pages/TablesPage.razor.js";

    // A phone's open sheet takes the lower half, so the map shrinks to stay whole above it
    private string PageStyle => !IsSplitView && SelectedTable is not null
        ? $"display: flex; gap: 12px; height: calc({ScreenHeight} - {SheetHeight});"
        : $"display: flex; gap: 12px; height: calc({ScreenHeight});";

    private string MapColumnStyle =>
        $"display: flex; flex-direction: column; gap: 12px; flex: {(IsSplitView ? 7 : 1)} 1 0; min-width: 0;";

    // At least the minimum cell size, and never smaller than the frame in either direction, so the grid fills it
    // (cqw and cqh are the frame's width and height)
    private string MapStyle => SelectedArea is null
        ? string.Empty
        : $"width: max({SelectedArea.Columns * MinCellSizeInPixels}px, 100cqw, " +
          $"calc(100cqh * {SelectedArea.Columns} / {SelectedArea.Rows}));";

    protected override async Task OnInitializedAsync()
    {
        OrdersHubService.OrdersChanged += OnOrdersChanged;
        OrdersHubService.TablesChanged += OnTablesChanged;

        _ = RefreshTimesAsync();

        await Task.WhenAll(GetFloorPlanAsync(), GetTablesStatusAsync(), GetRequestsAsync());

        await SelectTableFromQueryAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await BrowserViewportService.SubscribeAsync(
                _viewportSubscriptionId,
                OnViewportChangedAsync,
                options: null,
                fireImmediately: true);
        }

        if (_tableToScrollTo is not { } tableId)
        {
            return;
        }

        _tableToScrollTo = null;

        _module ??= await JsRuntime.InvokeAsync<IJSObjectReference>("import", ModulePath);

        await _module.InvokeVoidAsync("scrollToTable", MapScrollElement, tableId);
    }

    public async ValueTask DisposeAsync()
    {
        OrdersHubService.OrdersChanged -= OnOrdersChanged;
        OrdersHubService.TablesChanged -= OnTablesChanged;
        _timeRefreshTimer.Dispose();

        await BrowserViewportService.UnsubscribeAsync(_viewportSubscriptionId);

        if (_module is null)
        {
            return;
        }

        try
        {
            await _module.DisposeAsync();
        }
        catch (JSDisconnectedException)
        {
            // The browser is already gone, and the module with it
        }
    }

    private async Task OnViewportChangedAsync(BrowserViewportEventArgs args)
    {
        IsSplitView = args.Breakpoint is not Breakpoint.Xs;

        await InvokeAsync(StateHasChanged);
    }

    private void OnOrdersChanged()
    {
        _ = InvokeAsync(() => Task.WhenAll(GetTablesStatusAsync(), GetTableOrdersAsync(isLiveUpdate: true)));
    }

    // The floor plan or a guest's request changed
    private void OnTablesChanged()
    {
        _ = InvokeAsync(() => Task.WhenAll(GetFloorPlanAsync(isLiveUpdate: true), GetRequestsAsync()));
    }

    // A live update shows no skeleton, and a failed one keeps what is shown
    private async Task GetFloorPlanAsync(bool isLiveUpdate = false)
    {
        IReadOnlyList<AreaDto>? floorPlan = await ApiService.ExecuteAsync(() => TableApiClient.GetFloorPlanAsync());

        if (floorPlan is not null || !isLiveUpdate)
        {
            Areas = floorPlan ?? [];
        }

        IsLoadingFloorPlan = false;

        StateHasChanged();
    }

    // A failed reload keeps what is shown
    private async Task GetRequestsAsync()
    {
        IReadOnlyList<TableRequestDto>? requests =
            await ApiService.ExecuteAsync(() => TableApiClient.GetTableRequestsAsync());

        if (requests is null)
        {
            return;
        }

        Requests = requests;

        StateHasChanged();
    }

    private async Task CompleteRequestAsync(TableRequestDto request)
    {
        CompletingRequestId = request.Id;

        bool isSuccess = await ApiService.ExecuteAsync(
            () => TableApiClient.CompleteTableRequestAsync(request.Id),
            $"{request.TableCode} · {request.Type.GetLabel()} done");

        CompletingRequestId = null;

        if (isSuccess)
        {
            await GetRequestsAsync();
        }
    }

    private bool HasRequests(TableDto table)
    {
        return Requests.Any(request => request.TableId == table.Id);
    }

    private async Task GetTablesStatusAsync()
    {
        IReadOnlyList<TableStatusDto>? statuses =
            await ApiService.ExecuteAsync(() => TableApiClient.GetTablesStatusAsync());

        if (statuses is null)
        {
            return;
        }

        StatusesByTableId = statuses.ToDictionary(status => status.TableId);

        StateHasChanged();
    }

    // Pending, accepted and active orders of the selected table, oldest first
    private async Task GetTableOrdersAsync(bool isLiveUpdate = false)
    {
        if (SelectedTableId is not { } tableId)
        {
            return;
        }

        if (!isLiveUpdate)
        {
            IsLoadingTableOrders = true;
        }

        PaginationDto<OrderDto>?[] pages = await Task.WhenAll(OpenStatuses.Select(status =>
            ApiService.ExecuteAsync(() => OrderApiClient.GetOrdersAsync(
                1,
                MaxOrdersPerStatus,
                nameof(OrderSortBy.CreatedAtUtcAsc),
                status,
                tableId: tableId))));

        // Another table was tapped meanwhile
        if (SelectedTableId != tableId)
        {
            return;
        }

        if (pages.All(page => page is not null) || !isLiveUpdate)
        {
            TableOrders = pages
                .SelectMany(page => page?.Items ?? [])
                .OrderBy(order => order.CreatedAtUtc)
                .ToList();
        }

        IsLoadingTableOrders = false;

        StateHasChanged();
    }

    private async Task SelectTableFromQueryAsync()
    {
        if (string.IsNullOrWhiteSpace(TableCode))
        {
            return;
        }

        AreaDto? area = Areas.FirstOrDefault(area => area.Tables.Any(table => table.Code == TableCode));

        if (area is null)
        {
            return;
        }

        SelectedAreaId = area.Id;
        SelectedTableId = area.Tables.First(table => table.Code == TableCode).Id;
        _tableToScrollTo = SelectedTableId;

        await GetTableOrdersAsync();
    }

    private void SelectArea(Guid areaId)
    {
        SelectedAreaId = areaId;

        ClearSelection();
    }

    // Fixtures are only there to find the tables, so a tap on one changes nothing
    private async Task SelectTableAsync(Guid? pieceId)
    {
        if (pieceId is null)
        {
            ClearSelection();

            return;
        }

        if (SelectedArea!.Tables.All(table => table.Id != pieceId) || pieceId == SelectedTableId)
        {
            return;
        }

        SelectedTableId = pieceId;
        _tableToScrollTo = pieceId;
        TableOrders = [];

        await GetTableOrdersAsync();
    }

    private void ClearSelection()
    {
        SelectedTableId = null;
        TableOrders = [];
    }

    // A tablet opens it in the Orders page's panel, a phone on its own page
    private void OpenOrder(OrderDto order)
    {
        if (IsSplitView)
        {
            OrdersTabState.SelectedOrderId = order.Id;

            NavigationManager.NavigateTo("/");

            return;
        }

        NavigationManager.NavigateTo($"/orders/{order.Id}");
    }

    private TableStatusDto? GetStatus(TableDto table)
    {
        return StatusesByTableId.GetValueOrDefault(table.Id);
    }

    private static string GetStatusColor(TableStatusDto status)
    {
        if (DateTimeOffset.UtcNow - status.OldestOpenOrderPlacedAtUtc >= LongWait)
        {
            return ErrorColor;
        }

        return status.PendingCount > 0 ? WarningColor : InfoColor;
    }

    private string? GetTableStyle(TableDto table)
    {
        if (GetStatus(table) is not { } status)
        {
            return null;
        }

        string color = GetStatusColor(status);

        return $"border-color: var(--mud-palette-{color}); " +
               $"background-color: rgba(var(--mud-palette-{color}-rgb), 0.16);";
    }

    // Upright even on a rotated table
    private static string GetBadgeStyle(TableStatusDto status)
    {
        string color = GetStatusColor(status);

        return "position: absolute; top: 2px; right: 2px; writing-mode: horizontal-tb; " +
               "min-width: 18px; height: 18px; padding: 0 5px; border-radius: 9px; " +
               "display: flex; align-items: center; justify-content: center; " +
               "font-size: 11px; font-weight: 600; " +
               $"background-color: var(--mud-palette-{color}); color: var(--mud-palette-{color}-text);";
    }

    private static string GetLegendDotStyle(string color)
    {
        return "display: inline-block; width: 10px; height: 10px; border-radius: 50%; " +
               $"background-color: var(--mud-palette-{color});";
    }

    private string GetAreaButtonStyle(Guid areaId)
    {
        return areaId == SelectedArea?.Id ? ActiveAreaStyle : InactiveAreaStyle;
    }

    private async Task RefreshTimesAsync()
    {
        while (await _timeRefreshTimer.WaitForNextTickAsync())
        {
            await InvokeAsync(StateHasChanged);
        }
    }
}