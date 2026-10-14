using Microsoft.AspNetCore.Components;
using OrderPoint.Bar.Menu.Api;
using OrderPoint.Bar.Menu.Dtos;
using OrderPoint.Shared.Dtos;
using OrderPoint.Shared.Orders.Services;
using OrderPoint.Shared.Services;

namespace OrderPoint.Bar.Menu.Pages;

// The menu as the admin set it up, to answer guests; it follows changes live
public sealed partial class MenuPage : IDisposable
{
    private const int MaxCategoriesCount = 100;

    // Three rows of three cards on a phone
    private const int LoadingCardsCount = 9;

    // The search stays right under the dense app bar while scrolling, like the order tabs
    private const string SearchWrapperStyle =
        "position: sticky; top: 48px; z-index: 2; padding: 12px 0; margin-top: -12px; " +
        "background-color: var(--mud-palette-background);";

    [Inject]
    private ApiService ApiService { get; set; } = null!;

    [Inject]
    private MenuApiClient MenuApiClient { get; set; } = null!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    [Inject]
    private OrdersHubService OrdersHubService { get; set; } = null!;

    private IReadOnlyList<CategoryDto> Categories { get; set; } = [];

    private bool IsLoading { get; set; } = true;

    private string? SearchQuery { get; set; }

    protected override async Task OnInitializedAsync()
    {
        OrdersHubService.MenuChanged += OnMenuChanged;

        await GetCategoriesAsync();
    }

    public void Dispose()
    {
        OrdersHubService.MenuChanged -= OnMenuChanged;
    }

    private void OnMenuChanged()
    {
        _ = InvokeAsync(() => GetCategoriesAsync(isLiveUpdate: true));
    }

    // A live update shows no skeleton, and a failed one keeps what is shown
    private async Task GetCategoriesAsync(bool isLiveUpdate = false)
    {
        if (!isLiveUpdate)
        {
            IsLoading = true;
        }

        PaginationDto<CategoryDto>? categories =
            await ApiService.ExecuteAsync(() => MenuApiClient.GetCategoriesAsync(MaxCategoriesCount, SearchQuery));

        if (categories is not null || !isLiveUpdate)
        {
            Categories = categories?.Items ?? [];
        }

        IsLoading = false;

        StateHasChanged();
    }

    private async Task OnSearchChangedAsync()
    {
        await GetCategoriesAsync();
    }

    private void OpenCategory(CategoryDto category)
    {
        NavigationManager.NavigateTo($"/menu/{category.Id}");
    }
}