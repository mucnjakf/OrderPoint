using Microsoft.AspNetCore.Components;
using OrderPoint.Bartender.Menu.Api;
using OrderPoint.Bartender.Menu.Dtos;
using OrderPoint.Shared.Dtos;
using OrderPoint.Shared.Services;

namespace OrderPoint.Bartender.Menu.Pages;

// The menu as the admin set it up, to answer guests; it changes only in the Admin, so it is not live
public sealed partial class MenuPage
{
    private const int MaxCategoriesCount = 100;

    // Three rows of two cards on a phone
    private const int LoadingCardsCount = 6;

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

    private IReadOnlyList<CategoryDto> Categories { get; set; } = [];

    private bool IsLoading { get; set; } = true;

    private string? SearchQuery { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await GetCategoriesAsync();
    }

    private async Task GetCategoriesAsync()
    {
        IsLoading = true;

        PaginationDto<CategoryDto>? categories =
            await ApiService.ExecuteAsync(() => MenuApiClient.GetCategoriesAsync(MaxCategoriesCount, SearchQuery));

        Categories = categories?.Items ?? [];

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