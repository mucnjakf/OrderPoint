using Microsoft.AspNetCore.Components;
using OrderPoint.Customer.Orders.Services;
using OrderPoint.Customer.Tables.Api;
using OrderPoint.Customer.Tables.Api.Requests;
using OrderPoint.Customer.Tables.Services;
using OrderPoint.Shared.Services;
using OrderPoint.Shared.Tables.Dtos;
using OrderPoint.Shared.Tables.Enumerations;

namespace OrderPoint.Customer.Tables.Pages;

// Call a bartender or ask for the bill; live, so "on the way" disappears once a bartender marks it done
public sealed partial class MyTablePage : IDisposable
{
    // "Asked 3 min ago" moves on even when nothing changes
    private const int TimeRefreshIntervalInSeconds = 30;

    private readonly PeriodicTimer _timeRefreshTimer = new(TimeSpan.FromSeconds(TimeRefreshIntervalInSeconds));

    [Inject]
    private TableService TableService { get; set; } = null!;

    [Inject]
    private ApiService ApiService { get; set; } = null!;

    [Inject]
    private TableApiClient TableApiClient { get; set; } = null!;

    [Inject]
    private OrderTrackingHubService OrderTrackingHubService { get; set; } = null!;

    private IReadOnlyList<TableRequestDto> OpenRequests { get; set; } = [];

    private bool IsLoading { get; set; } = true;

    private TableRequestType? SendingType { get; set; }

    private string TableText => TableService.AreaName is null
        ? $"Table {TableService.TableCode}"
        : $"{TableService.AreaName} · {TableService.TableCode}";

    protected override async Task OnInitializedAsync()
    {
        OrderTrackingHubService.TablesChanged += OnTablesChanged;

        _ = RefreshTimesAsync();

        await GetRequestsAsync();
    }

    public void Dispose()
    {
        OrderTrackingHubService.TablesChanged -= OnTablesChanged;
        _timeRefreshTimer.Dispose();
    }

    private void OnTablesChanged()
    {
        _ = InvokeAsync(GetRequestsAsync);
    }

    // A failed reload keeps what is shown
    private async Task GetRequestsAsync()
    {
        if (TableService.TableCode is not { } tableCode)
        {
            return;
        }

        IReadOnlyList<TableRequestDto>? requests =
            await ApiService.ExecuteAsync(() => TableApiClient.GetTableRequestsAsync(tableCode));

        if (requests is not null)
        {
            OpenRequests = requests;
        }

        IsLoading = false;

        StateHasChanged();
    }

    private TableRequestDto? GetOpenRequest(TableRequestType type)
    {
        return OpenRequests.FirstOrDefault(request => request.Type == type);
    }

    // No snackbar: the card turns into "on the way"
    private async Task CreateRequestAsync(TableRequestType type)
    {
        SendingType = type;

        TableRequestDto? request = await ApiService.ExecuteAsync(() =>
            TableApiClient.CreateTableRequestAsync(TableService.TableCode!, new CreateTableRequestRequest(type)));

        SendingType = null;

        if (request is not null)
        {
            OpenRequests = [.. OpenRequests.Where(other => other.Id != request.Id), request];
        }
    }

    private async Task RefreshTimesAsync()
    {
        while (await _timeRefreshTimer.WaitForNextTickAsync())
        {
            await InvokeAsync(StateHasChanged);
        }
    }
}