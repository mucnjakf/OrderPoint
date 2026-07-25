using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Admin.Categories.Dtos;
using OrderPoint.Admin.Items.Api;
using OrderPoint.Admin.Items.Api.Requests;
using OrderPoint.Admin.Items.Dialogs;
using OrderPoint.Admin.Items.Dtos;
using OrderPoint.Admin.Shared.Dtos;
using OrderPoint.Admin.Shared.Services;

namespace OrderPoint.Admin.Categories.Dialogs;

public sealed partial class CategoryDetailsDialog
{
    [Parameter]
    public CategoryDto Category { get; set; } = null!;

    [CascadingParameter]
    private IMudDialogInstance MudDialogInstance { get; set; } = null!;

    [Inject]
    private ApiService ApiService { get; set; } = null!;

    [Inject]
    private ItemApiClient ItemApiClient { get; set; } = null!;

    [Inject]
    private IDialogService DialogService { get; set; } = null!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = null!;

    private string SelectedSortBy { get; set; } = "CreatedAtUtcDesc";

    private string? SearchQuery { get; set; }

    private bool IsLoading { get; set; } = true;

    private PaginationDto<ItemDto>? Pagination { get; set; }

    private IReadOnlyList<ItemDto> Items { get; set; } = [];

    protected override async Task OnParametersSetAsync()
    {
        await GetItemsAsync(
            1,
            5,
            SelectedSortBy,
            SearchQuery);
    }

    private async Task GetItemsAsync(
        int pageNumber,
        int pageSize,
        string sortBy,
        string? searchQuery)
    {
        IsLoading = true;

        Pagination = await ApiService.ExecuteAsync(async ()
            => await ItemApiClient.GetItemsAsync(
                pageNumber,
                pageSize,
                sortBy,
                searchQuery,
                Category.Id));

        Items = Pagination.Items;

        IsLoading = false;
    }

    private async Task OnSearchChangedAsync()
    {
        await GetItemsAsync(
            1,
            5,
            SelectedSortBy,
            SearchQuery);
    }

    private async Task OnSortChangedAsync()
    {
        await GetItemsAsync(
            1,
            5,
            SelectedSortBy,
            SearchQuery);
    }

    private async Task OnPageChanged(int pageNumber)
    {
        await GetItemsAsync(
            pageNumber,
            5,
            SelectedSortBy,
            SearchQuery);
    }

    private async Task ShowCreateItemDialogAsync()
    {
        var parameters = new DialogParameters<CreateItemDialog>
        {
            { dialog => dialog.Category, Category }
        };

        var options = new DialogOptions
        {
            MaxWidth = MaxWidth.Medium,
            FullWidth = true
        };

        IDialogReference dialogReference = await DialogService
            .ShowAsync<CreateItemDialog>(string.Empty, parameters, options);

        DialogResult dialogResult = (await dialogReference.Result)!;

        if (!dialogResult.Canceled)
        {
            var request = (dialogResult.Data as CreateItemRequest)!;

            await ApiService.ExecuteAsync(async ()
                => await ItemApiClient.CreateItemAsync(request));

            Snackbar.Add($"Item {request.Name} created successfully", Severity.Success);

            await GetItemsAsync(
                1,
                5,
                SelectedSortBy,
                SearchQuery);
        }
    }

    private async Task ShowDeleteItemDialogAsync(Guid id, string itemName)
    {
        var parameters = new DialogParameters<DeleteItemDialog>
        {
            { dialog => dialog.ItemName, itemName }
        };

        var options = new DialogOptions
        {
            MaxWidth = MaxWidth.Small,
            FullWidth = true
        };

        IDialogReference dialogReference = await DialogService
            .ShowAsync<DeleteItemDialog>(string.Empty, parameters, options);

        DialogResult dialogResult = (await dialogReference.Result)!;

        if (!dialogResult.Canceled)
        {
            await ApiService.ExecuteAsync(async ()
                => await ItemApiClient.DeleteItemAsync(id));

            Snackbar.Add($"Item {itemName} deleted successfully", Severity.Success);

            await GetItemsAsync(
                1,
                5,
                SelectedSortBy,
                SearchQuery);
        }
    }

    private void Close()
    {
        MudDialogInstance.Close(DialogResult.Ok(true));
    }
}