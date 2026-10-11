using System.Globalization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using OrderPoint.Admin.Bartenders.Api;
using OrderPoint.Admin.Bartenders.Dtos;
using OrderPoint.Admin.Categories.Api;
using OrderPoint.Admin.Categories.Dtos;
using OrderPoint.Admin.Items.Api;
using OrderPoint.Admin.Items.Dtos;
using OrderPoint.Admin.Items.Sorting;
using OrderPoint.Admin.Orders.Api;
using OrderPoint.Admin.Orders.Dtos;
using OrderPoint.Admin.Orders.Sorting;
using OrderPoint.Admin.Search.Dtos;
using OrderPoint.Admin.Search.Enumerations;
using OrderPoint.Admin.Search.Services;
using OrderPoint.Admin.Shared.Dtos;
using OrderPoint.Admin.Shared.Extensions;
using OrderPoint.Admin.Shared.Services;

namespace OrderPoint.Admin.Search.Dialogs;

public sealed partial class GlobalSearchDialog : IDisposable
{
    private const int ResultsPerGroupCount = 5;

    private static readonly IReadOnlyList<SearchResultDto> PageResults =
    [
        new(SearchResultKind.Page, "Dashboard", Href: "/", Icon: Icons.Material.Filled.Dashboard),
        new(SearchResultKind.Page, "Orders", Href: "/orders", Icon: Icons.Material.Filled.Receipt),
        new(SearchResultKind.Page, "Bartenders", Href: "/bartenders", Icon: Icons.Material.Filled.People),
        new(SearchResultKind.Page, "Items", Href: "/items", Icon: Icons.Material.Filled.MenuBook),
        new(SearchResultKind.Page, "Categories", Href: "/categories", Icon: Icons.Material.Filled.Category)
    ];

    [CascadingParameter]
    private IMudDialogInstance MudDialogInstance { get; set; } = null!;

    [Inject]
    private TimeZoneService TimeZoneService { get; set; } = null!;

    [Inject]
    private ApiService ApiService { get; set; } = null!;

    [Inject]
    private SearchService SearchService { get; set; } = null!;

    [Inject]
    private OrderApiClient OrderApiClient { get; set; } = null!;

    [Inject]
    private ItemApiClient ItemApiClient { get; set; } = null!;

    [Inject]
    private CategoryApiClient CategoryApiClient { get; set; } = null!;

    [Inject]
    private BartenderApiClient BartenderApiClient { get; set; } = null!;

    private string? SearchQuery { get; set; }

    private CancellationTokenSource? SearchCancellation { get; set; }

    private IReadOnlyList<SearchResultDto> RecentResults { get; set; } = [];

    private IReadOnlyList<SearchResultDto> OrderResults { get; set; } = [];

    private IReadOnlyList<SearchResultDto> ItemResults { get; set; } = [];

    private IReadOnlyList<SearchResultDto> CategoryResults { get; set; } = [];

    private IReadOnlyList<SearchResultDto> BartenderResults { get; set; } = [];

    private bool IsLoadingOrders { get; set; }

    private bool IsLoadingItems { get; set; }

    private bool IsLoadingCategories { get; set; }

    private bool IsLoadingBartenders { get; set; }

    private SearchResultDto? ActiveResult { get; set; }

    private bool HasQuery => !string.IsNullOrWhiteSpace(SearchQuery);

    private bool IsSearching => IsLoadingOrders || IsLoadingItems || IsLoadingCategories || IsLoadingBartenders;

    private bool HasNoResults => !IsSearching && VisibleResults.Count == 0;

    private IReadOnlyList<SearchResultDto> VisibleResults => HasQuery
        ? [.. OrderResults, .. ItemResults, .. CategoryResults, .. BartenderResults]
        : [.. RecentResults, .. PageResults];

    protected override async Task OnInitializedAsync()
    {
        RecentResults = await SearchService.GetRecentResultsAsync();

        ActiveResult = VisibleResults.FirstOrDefault();
    }

    public void Dispose()
    {
        SearchCancellation?.Cancel();
        SearchCancellation?.Dispose();
    }

    private async Task OnSearchQueryChangedAsync()
    {
        SearchCancellation?.Cancel();
        SearchCancellation?.Dispose();
        SearchCancellation = new CancellationTokenSource();

        OrderResults = [];
        ItemResults = [];
        CategoryResults = [];
        BartenderResults = [];
        ActiveResult = null;

        if (!HasQuery)
        {
            ActiveResult = VisibleResults.FirstOrDefault();
            return;
        }

        string searchQuery = SearchQuery!;
        CancellationToken cancellationToken = SearchCancellation.Token;

        await Task.WhenAll(
            SearchOrdersAsync(searchQuery, cancellationToken),
            SearchItemsAsync(searchQuery, cancellationToken),
            SearchCategoriesAsync(searchQuery, cancellationToken),
            SearchBartendersAsync(searchQuery, cancellationToken));
    }

    private async Task SearchOrdersAsync(string searchQuery, CancellationToken cancellationToken)
    {
        IsLoadingOrders = true;

        PaginationDto<OrderDto>? pagination = await ApiService.ExecuteAsync(
            () => OrderApiClient.GetOrdersAsync(
                pageNumber: 1,
                ResultsPerGroupCount,
                nameof(OrderSortBy.CreatedAtUtcDesc),
                searchQuery,
                cancellationToken: cancellationToken),
            cancellationToken);

        if (cancellationToken.IsCancellationRequested)
        {
            return;
        }

        OrderResults = [.. (pagination?.Items ?? []).Select(ToSearchResult)];
        IsLoadingOrders = false;

        OnResultsLoaded();
    }

    private async Task SearchItemsAsync(string searchQuery, CancellationToken cancellationToken)
    {
        IsLoadingItems = true;

        PaginationDto<ItemDto>? pagination = await ApiService.ExecuteAsync(
            () => ItemApiClient.GetItemsAsync(
                pageNumber: 1,
                ResultsPerGroupCount,
                nameof(ItemSortBy.NameAsc),
                searchQuery,
                cancellationToken: cancellationToken),
            cancellationToken);

        if (cancellationToken.IsCancellationRequested)
        {
            return;
        }

        ItemResults = [.. (pagination?.Items ?? []).Select(ToSearchResult)];
        IsLoadingItems = false;

        OnResultsLoaded();
    }

    private async Task SearchCategoriesAsync(string searchQuery, CancellationToken cancellationToken)
    {
        IsLoadingCategories = true;

        IReadOnlyList<CategoryDto>? categories = await ApiService.ExecuteAsync(
            () => CategoryApiClient.SearchCategoriesAsync(searchQuery, cancellationToken),
            cancellationToken);

        if (cancellationToken.IsCancellationRequested)
        {
            return;
        }

        CategoryResults = [.. (categories ?? []).Take(ResultsPerGroupCount).Select(ToSearchResult)];
        IsLoadingCategories = false;

        OnResultsLoaded();
    }

    private async Task SearchBartendersAsync(string searchQuery, CancellationToken cancellationToken)
    {
        IsLoadingBartenders = true;

        IReadOnlyList<BartenderDto>? bartenders = await ApiService.ExecuteAsync(
            () => BartenderApiClient.SearchBartendersAsync(searchQuery, cancellationToken),
            cancellationToken);

        if (cancellationToken.IsCancellationRequested)
        {
            return;
        }

        BartenderResults = [.. (bartenders ?? []).Take(ResultsPerGroupCount).Select(ToSearchResult)];
        IsLoadingBartenders = false;

        OnResultsLoaded();
    }

    private void OnResultsLoaded()
    {
        ActiveResult ??= VisibleResults.FirstOrDefault();

        StateHasChanged();
    }

    private async Task OnKeyDownAsync(KeyboardEventArgs args)
    {
        IReadOnlyList<SearchResultDto> results = VisibleResults;

        if (results.Count == 0)
        {
            return;
        }

        int activeIndex = ActiveResult is null ? -1 : results.ToList().IndexOf(ActiveResult);

        switch (args.Key)
        {
            case "ArrowDown":
                ActiveResult = results[Math.Min(activeIndex + 1, results.Count - 1)];
                break;
            case "ArrowUp":
                ActiveResult = results[Math.Max(activeIndex - 1, 0)];
                break;
            case "Enter" when ActiveResult is not null:
                await Select(ActiveResult);
                break;
        }
    }

    private void Activate(SearchResultDto result)
    {
        ActiveResult = result;
    }

    private Task Select(SearchResultDto result)
    {
        MudDialogInstance.Close(DialogResult.Ok(result));

        return Task.CompletedTask;
    }

    private SearchResultDto ToSearchResult(OrderDto order)
    {
        string createdAtText = order.CreatedAtUtc.ToDisplayDateTime(TimeZoneService.TimeZone);

        return new SearchResultDto(
            SearchResultKind.Order,
            $"Table {order.TableCode}",
            $"{createdAtText} · {order.Status}",
            order.Total.ToString("C", CultureInfo.CurrentCulture),
            order.Id,
            OrderNumber: order.Number);
    }

    private static SearchResultDto ToSearchResult(ItemDto item)
    {
        return new SearchResultDto(
            SearchResultKind.Item,
            item.Name,
            item.Category.Name,
            item.Price.ToString("C", CultureInfo.CurrentCulture),
            item.Id,
            item.ImageUrl);
    }

    private static SearchResultDto ToSearchResult(CategoryDto category)
    {
        return new SearchResultDto(
            SearchResultKind.Category,
            category.Name,
            category.ItemsCount.ToCountText("item", "items"),
            Id: category.Id,
            ImageUrl: category.ImageUrl);
    }

    private static SearchResultDto ToSearchResult(BartenderDto bartender)
    {
        return new SearchResultDto(
            SearchResultKind.Bartender,
            $"{bartender.FirstName} {bartender.LastName}",
            bartender.Email,
            Id: bartender.Id,
            ImageUrl: bartender.ImageUrl);
    }
}