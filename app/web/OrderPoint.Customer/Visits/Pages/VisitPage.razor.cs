using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Customer.Orders.Services;
using OrderPoint.Customer.Settings.Dialogs;
using OrderPoint.Customer.Settings.Enumerations;
using OrderPoint.Customer.Settings.Services;
using OrderPoint.Customer.Tables.Api;
using OrderPoint.Customer.Tables.Api.Requests;
using OrderPoint.Customer.Tables.Services;
using OrderPoint.Shared.Services;
using OrderPoint.Shared.Tables.Dtos;
using OrderPoint.Shared.Tables.Enumerations;

namespace OrderPoint.Customer.Visits.Pages;

// Everything about this visit: the table, calling a bartender (live, so "on the way" disappears once a bartender marks
// it done), and the settings kept on this phone
public sealed partial class VisitPage : IDisposable
{
    // "Asked 3 min ago" moves on even when nothing changes
    private const int TimeRefreshIntervalInSeconds = 30;

    private static readonly (AppearanceMode Mode, string Icon)[] AppearanceModes =
    [
        (AppearanceMode.Auto, Icons.Material.Filled.BrightnessAuto),
        (AppearanceMode.Light, Icons.Material.Filled.LightMode),
        (AppearanceMode.Dark, Icons.Material.Filled.DarkMode)
    ];

    private readonly PeriodicTimer _timeRefreshTimer = new(TimeSpan.FromSeconds(TimeRefreshIntervalInSeconds));

    [Inject]
    private TableService TableService { get; set; } = null!;

    [Inject]
    private ApiService ApiService { get; set; } = null!;

    [Inject]
    private TableApiClient TableApiClient { get; set; } = null!;

    [Inject]
    private OrderTrackingHubService OrderTrackingHubService { get; set; } = null!;

    [Inject]
    private AppearanceService AppearanceService { get; set; } = null!;

    [Inject]
    private MyOrdersService MyOrdersService { get; set; } = null!;

    [Inject]
    private IDialogService DialogService { get; set; } = null!;

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

    private async Task OnAppearanceModeChangedAsync(AppearanceMode mode)
    {
        await AppearanceService.SetAsync(mode);
    }

    // Only hides them on this phone; the orders themselves stay with the bar
    private async Task ForgetOrdersAsync()
    {
        var parameters = new DialogParameters<ForgetOrdersDialog>
        {
            { dialog => dialog.OrdersCount, MyOrdersService.OrderIds.Count }
        };

        var options = new DialogOptions
        {
            MaxWidth = MaxWidth.ExtraSmall,
            FullWidth = true
        };

        IDialogReference dialogReference = await DialogService
            .ShowAsync<ForgetOrdersDialog>(string.Empty, parameters, options);

        DialogResult dialogResult = (await dialogReference.Result)!;

        if (dialogResult.Canceled)
        {
            return;
        }

        await MyOrdersService.ClearAsync();
    }

    private async Task RefreshTimesAsync()
    {
        while (await _timeRefreshTimer.WaitForNextTickAsync())
        {
            await InvokeAsync(StateHasChanged);
        }
    }
}