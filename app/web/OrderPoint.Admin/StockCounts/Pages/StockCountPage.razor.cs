using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Admin.Shared.Extensions;
using OrderPoint.Admin.Stock.Enumerations;
using OrderPoint.Admin.StockCounts.Api;
using OrderPoint.Admin.StockCounts.Api.Requests;
using OrderPoint.Admin.StockCounts.Dialogs;
using OrderPoint.Admin.StockCounts.Dtos;
using OrderPoint.Admin.StockCounts.Enumerations;
using OrderPoint.Admin.StockCounts.Sorting;
using OrderPoint.Shared.Dtos;
using OrderPoint.Shared.Extensions;
using OrderPoint.Shared.Orders.Services;
using OrderPoint.Shared.Services;

namespace OrderPoint.Admin.StockCounts.Pages;

// The count sheet: every figure saves when its field is left; searching, sorting and paging happen here, since the
// whole count is loaded
public sealed partial class StockCountPage : IDisposable
{
    private const int PageSize = 50;

    private const string ColumnTitleStyle = "color: var(--mud-palette-text-secondary)";

    private const string FooterStyle = "position: sticky; bottom: 12px; z-index: 2;";

    private static readonly int AllGroupsCount = Enum.GetValues<StockGroup>().Length;

    // One save at a time, sending what the fields hold then, so an earlier save can't overtake a later one
    private readonly SemaphoreSlim _saveLock = new(1, 1);

    private int _loadVersion;

    [Inject]
    private IDialogService DialogService { get; set; } = null!;

    [Inject]
    private ApiService ApiService { get; set; } = null!;

    [Inject]
    private StockCountApiClient StockCountApiClient { get; set; } = null!;

    [Inject]
    private OrdersHubService OrdersHubService { get; set; } = null!;

    [Inject]
    private TimeZoneService TimeZoneService { get; set; } = null!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    [Parameter]
    public Guid Id { get; set; }

    private StockCountDto? StockCount { get; set; }

    private IReadOnlyList<StockCountEntryDto> Entries { get; set; } = [];

    private IReadOnlyList<StockCountEntryDto> PageEntries { get; set; } = [];

    private PaginationDto<StockCountEntryDto>? Pagination { get; set; }

    private int PageNumber { get; set; } = 1;

    private string SelectedSortBy { get; set; } = nameof(StockCountLineSortBy.NameAsc);

    private string? SearchQuery { get; set; }

    private bool IsOnlyNotCounted { get; set; }

    private bool IsLoading { get; set; } = true;

    private bool IsClosed => StockCount?.Status is StockCountStatus.Closed;

    private bool IsSaving => Entries.Any(entry => entry.IsSaving);

    private int CountedCount => Entries.Count(entry => entry.Line.CountedQuantity is not null);

    private double ProgressPercent => Entries.Count == 0 ? 0 : 100.0 * CountedCount / Entries.Count;

    private decimal ValueChange => Entries.Sum(entry => entry.Line.ValueChange ?? 0);

    // "18 not counted stay as they are"
    private string NotCountedText
    {
        get
        {
            int notCountedCount = Entries.Count - CountedCount;

            return notCountedCount == 0
                ? "Everything counted"
                : $"{notCountedCount} not counted stay as they are";
        }
    }

    protected override async Task OnInitializedAsync()
    {
        OrdersHubService.OrdersChanged += OnOrdersChanged;

        await GetStockCountAsync();
    }

    public void Dispose()
    {
        OrdersHubService.OrdersChanged -= OnOrdersChanged;

        _saveLock.Dispose();
    }

    // Sales move what the lines not counted yet expect
    private void OnOrdersChanged()
    {
        _ = InvokeAsync(() => GetStockCountAsync(isLiveUpdate: true));
    }

    private async Task GetStockCountAsync(bool isLiveUpdate = false)
    {
        int loadVersion = ++_loadVersion;

        StockCountDto? stockCount = await ApiService.ExecuteAsync(() => StockCountApiClient.GetStockCountAsync(Id));

        // A newer load started meanwhile; a failed live update keeps what is shown
        if (loadVersion != _loadVersion || (isLiveUpdate && stockCount is null))
        {
            return;
        }

        StockCount = stockCount;

        MergeEntries(stockCount?.Lines ?? []);
        RefreshPage();

        IsLoading = false;

        StateHasChanged();
    }

    // Keeps each line's entry, and so what is typed into its fields
    private void MergeEntries(IReadOnlyList<StockCountLineDto> lines)
    {
        Dictionary<Guid, StockCountEntryDto> entriesById = Entries.ToDictionary(entry => entry.Line.Id);

        Entries = lines
            .Select(line =>
            {
                if (!entriesById.TryGetValue(line.Id, out StockCountEntryDto? entry))
                {
                    return new StockCountEntryDto(line);
                }

                entry.Line = line;

                return entry;
            })
            .ToList();
    }

    private void RefreshPage()
    {
        List<StockCountEntryDto> entries = GetFilteredEntries();

        int totalPages = Math.Max((int)Math.Ceiling(entries.Count / (double)PageSize), 1);
        PageNumber = Math.Min(PageNumber, totalPages);

        PageEntries = entries.Skip((PageNumber - 1) * PageSize).Take(PageSize).ToList();
        Pagination = new PaginationDto<StockCountEntryDto>(PageEntries, PageNumber, PageSize, entries.Count);
    }

    // By group, then by name or by difference (the biggest loss first, lines not counted last)
    private List<StockCountEntryDto> GetFilteredEntries()
    {
        IEnumerable<StockCountEntryDto> entries = Entries;

        if (!string.IsNullOrWhiteSpace(SearchQuery))
        {
            entries = entries.Where(entry =>
                entry.Line.StockItemName.Contains(SearchQuery.Trim(), StringComparison.CurrentCultureIgnoreCase));
        }

        if (IsOnlyNotCounted)
        {
            entries = entries.Where(entry => entry.Line.CountedQuantity is null);
        }

        IOrderedEnumerable<StockCountEntryDto> orderedEntries = entries.OrderBy(entry => entry.Line.Group);

        if (SelectedSortBy == nameof(StockCountLineSortBy.ValueChangeAsc))
        {
            orderedEntries = orderedEntries
                .ThenBy(entry => entry.Line.ValueChange is null)
                .ThenBy(entry => entry.Line.ValueChange);
        }

        return orderedEntries.ThenBy(entry => entry.Line.StockItemName).ToList();
    }

    private void OnFiltersChanged()
    {
        PageNumber = 1;

        RefreshPage();
    }

    private void OnPageChanged(int pageNumber)
    {
        PageNumber = pageNumber;

        RefreshPage();
    }

    private async Task SaveEntryAsync(StockCountEntryDto entry)
    {
        entry.IsSaving = true;

        await _saveLock.WaitAsync();

        try
        {
            bool isSuccess = await ApiService.ExecuteAsync<bool>(async () =>
            {
                await StockCountApiClient.CountStockLineAsync(
                    Id,
                    entry.Line.Id,
                    new CountStockLineRequest(entry.Quantity));

                return true;
            });

            if (!isSuccess)
            {
                entry.Reset();
            }

            await GetStockCountAsync(isLiveUpdate: true);
        }
        finally
        {
            _saveLock.Release();
        }

        entry.IsSaving = false;

        StateHasChanged();
    }

    private async Task ShowCloseStockCountDialogAsync()
    {
        if (StockCount is null)
        {
            return;
        }

        var parameters = new DialogParameters<CloseStockCountDialog>
        {
            { dialog => dialog.StockCount, StockCount }
        };

        IDialogReference dialogReference = await DialogService
            .ShowAsync<CloseStockCountDialog>(string.Empty, parameters);

        DialogResult dialogResult = (await dialogReference.Result)!;

        if (dialogResult.Canceled)
        {
            return;
        }

        var request = (dialogResult.Data as CloseStockCountRequest)!;

        bool isSuccess = await ApiService.ExecuteAsync(
            () => StockCountApiClient.CloseStockCountAsync(Id, request),
            $"Count closed: {ValueChange.ToSignedMoneyText()}");

        if (isSuccess)
        {
            NavigationManager.NavigateTo("/counts");
        }
    }

    private async Task ShowDiscardStockCountDialogAsync()
    {
        if (StockCount is null)
        {
            return;
        }

        var parameters = new DialogParameters<DiscardStockCountDialog>
        {
            { dialog => dialog.StockCount, StockCount }
        };

        IDialogReference dialogReference = await DialogService
            .ShowAsync<DiscardStockCountDialog>(string.Empty, parameters);

        DialogResult dialogResult = (await dialogReference.Result)!;

        if (dialogResult.Canceled)
        {
            return;
        }

        bool isSuccess = await ApiService.ExecuteAsync(
            () => StockCountApiClient.DiscardStockCountAsync(Id),
            "Count discarded");

        if (isSuccess)
        {
            NavigationManager.NavigateTo("/counts");
        }
    }

    // "12 stock items · 8 counted", for the rows on this page
    private static string GetGroupText(IReadOnlyList<StockCountEntryDto> entries)
    {
        int countedCount = entries.Count(entry => entry.Line.CountedQuantity is not null);

        return $"{entries.Count.ToCountText("stock item", "stock items")} · {countedCount} counted";
    }
}