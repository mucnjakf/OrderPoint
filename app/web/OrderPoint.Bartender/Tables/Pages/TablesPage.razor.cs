using Microsoft.AspNetCore.Components;
using OrderPoint.Bartender.Tables.Api;
using OrderPoint.Shared.Extensions;
using OrderPoint.Shared.Orders.Services;
using OrderPoint.Shared.Services;
using OrderPoint.Shared.Tables.Dtos;

namespace OrderPoint.Bartender.Tables.Pages;

// Where to take an order: the counter or a table, by its code; guests' calls on top
public sealed partial class TablesPage : IDisposable
{
    // "Asked 3 min ago" moves on even when nothing changes
    private const int TimeRefreshIntervalInSeconds = 30;

    private readonly PeriodicTimer _timeRefreshTimer = new(TimeSpan.FromSeconds(TimeRefreshIntervalInSeconds));

    [Inject]
    private ApiService ApiService { get; set; } = null!;

    [Inject]
    private TableApiClient TableApiClient { get; set; } = null!;

    [Inject]
    private OrdersHubService OrdersHubService { get; set; } = null!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    private IReadOnlyList<AreaDto> Areas { get; set; } = [];

    private IReadOnlyList<TableRequestDto> Requests { get; set; } = [];

    private bool IsLoading { get; set; } = true;

    private Guid? CompletingRequestId { get; set; }

    private IEnumerable<AreaDto> AreasWithTables => Areas.Where(area => area.Tables.Count != 0);

    protected override async Task OnInitializedAsync()
    {
        OrdersHubService.TablesChanged += OnTablesChanged;

        _ = RefreshTimesAsync();

        await Task.WhenAll(GetFloorPlanAsync(), GetRequestsAsync());
    }

    public void Dispose()
    {
        OrdersHubService.TablesChanged -= OnTablesChanged;
        _timeRefreshTimer.Dispose();
    }

    // The floor plan or a guest's call changed
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

        IsLoading = false;

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

    private bool HasCall(TableDto table)
    {
        return Requests.Any(request => request.TableId == table.Id);
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

    // A guest called: take their order
    private void OpenRequestTable(TableRequestDto request)
    {
        NavigationManager.NavigateTo($"/tables/{Uri.EscapeDataString(request.TableCode)}/order");
    }

    private void OpenTable(TableDto table)
    {
        NavigationManager.NavigateTo($"/tables/{Uri.EscapeDataString(table.Code)}/order");
    }

    private void OpenCounter()
    {
        NavigationManager.NavigateTo("/counter/order");
    }

    private async Task RefreshTimesAsync()
    {
        while (await _timeRefreshTimer.WaitForNextTickAsync())
        {
            await InvokeAsync(StateHasChanged);
        }
    }
}