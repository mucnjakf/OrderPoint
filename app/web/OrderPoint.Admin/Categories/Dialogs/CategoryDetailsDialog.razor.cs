using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Admin.Categories.Dtos;
using OrderPoint.Admin.Items.Api;
using OrderPoint.Admin.Items.Api.Requests;
using OrderPoint.Admin.Items.Dialogs;
using OrderPoint.Admin.Items.Dtos;
using OrderPoint.Admin.Items.Sorting;
using OrderPoint.Admin.Shared.Dtos;
using OrderPoint.Admin.Shared.Extensions;
using OrderPoint.Admin.Shared.Services;

namespace OrderPoint.Admin.Categories.Dialogs;

public sealed partial class CategoryDetailsDialog
{
    private const int PageSize = 5;

    [Parameter]
    public CategoryDto Category { get; set; } = null!;

    [Parameter]
    public EventCallback OnItemsChanged { get; set; }

    [CascadingParameter]
    private IMudDialogInstance MudDialogInstance { get; set; } = null!;

    [Inject]
    private TimeZoneService TimeZoneService { get; set; } = null!;

    [Inject]
    private IDialogService DialogService { get; set; } = null!;

    [Inject]
    private ApiService ApiService { get; set; } = null!;

    [Inject]
    private ItemApiClient ItemApiClient { get; set; } = null!;

    private PaginationDto<ItemDto>? Pagination { get; set; }

    private IReadOnlyList<ItemDto> Items { get; set; } = [];

    private string UpdatedAtText => Category.UpdatedAtUtc?.ToDisplayDateTime(TimeZoneService.TimeZone) ?? "-";

    private string SelectedSortBy { get; set; } = nameof(ItemSortBy.CreatedAtUtcDesc);

    private string? SearchQuery { get; set; }

    private bool IsLoading { get; set; } = true;

    protected override async Task OnInitializedAsync()
    {
        await GetItemsAsync(pageNumber: 1);
    }

    private async Task GetItemsAsync(int pageNumber)
    {
        IsLoading = true;

        Pagination = await ApiService.ExecuteAsync(() => ItemApiClient.GetItemsAsync(
            pageNumber,
            PageSize,
            SelectedSortBy,
            SearchQuery,
            Category.Id));

        Items = Pagination?.Items ?? [];

        IsLoading = false;

        StateHasChanged();
    }

    private async Task OnSearchChangedAsync()
    {
        await GetItemsAsync(pageNumber: 1);
    }

    private async Task OnSortChangedAsync()
    {
        await GetItemsAsync(pageNumber: 1);
    }

    private async Task OnPageChangedAsync(int pageNumber)
    {
        await GetItemsAsync(pageNumber);
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

        if (dialogResult.Canceled)
        {
            return;
        }

        var request = (dialogResult.Data as CreateItemRequest)!;

        bool isSuccess = await ApiService.ExecuteAsync(
            () => CreateItemWithImageAsync(request),
            $"Item {request.Name} created successfully");

        if (isSuccess)
        {
            await Task.WhenAll(
                GetItemsAsync(pageNumber: 1),
                OnItemsChanged.InvokeAsync());
        }
    }

    private async Task CreateItemWithImageAsync(CreateItemRequest request)
    {
        ItemDto item = await ItemApiClient.CreateItemAsync(request);

        if (request.Image is null)
        {
            return;
        }

        await ItemApiClient.UpdateItemImageAsync(item.Id, request.Image);
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

        if (dialogResult.Canceled)
        {
            return;
        }

        bool isSuccess = await ApiService.ExecuteAsync(
            () => ItemApiClient.DeleteItemAsync(id),
            $"Item {itemName} deleted successfully");

        if (isSuccess)
        {
            await Task.WhenAll(
                GetItemsAsync(pageNumber: 1),
                OnItemsChanged.InvokeAsync());
        }
    }

    private void Close()
    {
        MudDialogInstance.Close(DialogResult.Ok(true));
    }
}